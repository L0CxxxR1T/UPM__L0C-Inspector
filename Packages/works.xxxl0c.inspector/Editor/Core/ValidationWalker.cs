using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 1行に対応するプロパティを起点に SerializedProperty ツリーを歩き、検証ルールを実行する。
    /// 構造をたどる責務をここに集約し、ルール側は渡された1プロパティだけを見る形にしている。
    /// </summary>
    internal static class ValidationWalker
    {
        // [SerializeReference] は循環し得るので打ち切る。自前展開の深さ上限（InspectionLimits）とは別の、
        // 純粋な安全装置なので大きめに取る
        private const int MAX_DEPTH = 10;
        private const string ATTRIBUTE_SUFFIX = "Attribute";

        /// <summary>
        /// 起点のプロパティ以下を検証し、結果を results に詰める。
        /// </summary>
        /// <param name="hasOwnRow">
        /// 自前展開されて独立した行を持つプロパティを判定する述語（インスペクタ限定。無ければ null で可）。
        /// 該当する子には降りない（その行が自分で report.Collect を呼ぶので、二重に検証しない）。
        /// </param>
        public static void Collect(
            SerializedProperty root,
            FieldInfo rootField,
            List<ValidationMessage> results,
            Predicate<string> hasOwnRow)
        {
            WalkState state = new WalkState(root.propertyPath, root.displayName, rootField, results, hasOwnRow);
            // 起点フィールドは常に直接編集できる（トップレベル、または既に独立した行を持つネスト）ので
            // isInsideExpandedParent は true 扱いにして、無用な「反映されない」警告を出さない
            Visit(root, rootField, rootField.FieldType, state, 0, isInsideExpandedParent: true, CollectionRole.None);
            RemoveDuplicates(results);
        }

        /// <param name="attributeSource">property に適用される属性の取得元。コレクションの要素は親フィールドを引き継ぐ。</param>
        /// <param name="valueType">property の値の型。子フィールドを解決するのに使う。</param>
        /// <param name="isInsideExpandedParent">
        /// property の直近の親クラスが自前展開される対象か。
        /// true なら property の装飾属性はインスペクタで実際に反映されるので「反映されない」警告を出さない。
        /// インスペクタの `hasOwnRow` に頼らず独立に判定することで、一括チェックでも同じ結果になる。
        /// </param>
        /// <param name="role">attributeSource のフィールドから見た property の位置。</param>
        private static void Visit(
            SerializedProperty property,
            FieldInfo attributeSource,
            Type valueType,
            WalkState state,
            int depth,
            bool isInsideExpandedParent,
            CollectionRole role)
        {
            if (depth > MAX_DEPTH) return;

            // Dictionary も SerializedProperty 上は配列なので、配列より先に判定する
            if (DictionaryUtility.IsSerializedDictionary(valueType))
            {
                VisitDictionary(property, attributeSource, valueType, state, depth, isInsideExpandedParent);
                return;
            }

            // コンテナ自体は Collection、要素は Element としてそれぞれルールを実行する。
            // 要素単位のルール（[Required] など）は Collection を無視するので、空リストでは何も報告されない
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                VisitCollection(property, attributeSource, valueType, state, depth, isInsideExpandedParent);
                return;
            }

            RunRules(property, attributeSource, state, isInsideExpandedParent, role, valueType);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                Type concreteType = SerializedFieldUtility.ResolveManagedReferenceType(property);
                if (concreteType != null) valueType = concreteType;
            }
            else if (property.propertyType != SerializedPropertyType.Generic)
            {
                // 末端の値。これ以上降りるものが無い
                return;
            }

            if (!InspectedTypeCache.ContainsHandledAttributes(valueType)) return;

            // この階層がインスペクタで自前展開されるかを、hasOwnRow に頼らず独立に判定する。
            // InspectorBuilder.CanExpand と同じ基準（[SerializeReference] は対象外、Drawer 持ちは対象外、深さ上限）。
            // コレクションの要素は1行ずつ展開せず PropertyField の描画に任せるので、その中身も展開されない
            bool childrenExpandable = role == CollectionRole.None
                && property.propertyType != SerializedPropertyType.ManagedReference
                && depth < InspectionLimits.MAX_NESTED_DEPTH
                && !PropertyDrawerRegistry.HasDrawer(valueType);

            foreach (SerializedProperty child in SerializedFieldUtility.EnumerateDirectChildren(property))
            {
                // 自前展開された子は独立した行を持つので、そちらに任せる（二重検証を避ける）
                if (state.HasOwnRow(child.propertyPath)) continue;

                FieldInfo childField = SerializedFieldUtility.FindSerializedField(valueType, child.name);

                // Unity 内部フィールドなど、C# 側に対応が無いものは想定内のスキップ
                if (childField == null) continue;

                // 非表示のフィールドは検証しない（設定できないものを未設定だと責めない）。
                // インスペクタで自前展開されなかったネスト（カスタム PropertyDrawer 持ちなど）の奥にある
                // [ShowIf] もここで初めて評価されるので、3層の結果が揃う
                string childLabel = BuildLabel(child, state);
                if (!VisibilityEvaluator.IsVisible(child, childField, childLabel, state.Results)) continue;
                if (!VisibilityEvaluator.IsEnabled(child, childField, childLabel, state.Results)) continue;

                Visit(child, childField, childField.FieldType, state, depth + 1, childrenExpandable, CollectionRole.None);
            }
        }

        private static void VisitCollection(
            SerializedProperty property,
            FieldInfo attributeSource,
            Type valueType,
            WalkState state,
            int depth,
            bool isInsideExpandedParent)
        {
            RunRules(property, attributeSource, state, isInsideExpandedParent, CollectionRole.Collection, valueType);

            Type elementType = SerializedFieldUtility.GetCollectionElementType(valueType);
            if (elementType == null) return;

            bool sourceHasAttribute = InspectedTypeCache.HasHandledAttribute(attributeSource);
            bool elementHasAttribute = InspectedTypeCache.ContainsHandledAttributes(elementType);
            if (!sourceHasAttribute && !elementHasAttribute) return;

            for (int i = 0; i < property.arraySize; i++)
            {
                Visit(
                    property.GetArrayElementAtIndex(i), attributeSource, elementType, state, depth + 1,
                    isInsideExpandedParent, CollectionRole.Element);
            }
        }

        /// <summary>
        /// Dictionary は属性の有無に関わらず重複キーを検査する。重複した要素は実行時に黙って捨てられるため。
        /// 要素のキーと値は、Dictionary フィールドの属性を引き継いで検証する。
        /// </summary>
        private static void VisitDictionary(
            SerializedProperty property,
            FieldInfo attributeSource,
            Type dictionaryType,
            WalkState state,
            int depth,
            bool isInsideExpandedParent)
        {
            DictionaryUtility.TryGetKeyValueTypes(dictionaryType, out Type keyType, out Type valueType);
            string label = BuildLabel(property, state);

            RunRules(property, attributeSource, state, isInsideExpandedParent, CollectionRole.Dictionary, dictionaryType);
            ReportDuplicateKeys(property, label, state);

            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty entry = property.GetArrayElementAtIndex(i);
                SerializedProperty key = entry.FindPropertyRelative(DictionaryUtility.KEY_NAME);
                SerializedProperty value = entry.FindPropertyRelative(DictionaryUtility.VALUE_NAME);

                // "[0].value._x" ではなく、キーで要素を指すラベルにする
                string entryLabel = $"{label}[{DictionaryUtility.FormatKey(key, i)}]";
                state.RegisterLabel(key.propertyPath, entryLabel);
                state.RegisterLabel(value.propertyPath, entryLabel);

                Visit(key, attributeSource, keyType, state, depth + 1, isInsideExpandedParent, CollectionRole.DictionaryKey);
                Visit(value, attributeSource, valueType, state, depth + 1, isInsideExpandedParent, CollectionRole.DictionaryValue);
            }
        }

        private static void ReportDuplicateKeys(SerializedProperty dictionary, string label, WalkState state)
        {
            List<List<int>> duplicates;
            try
            {
                duplicates = DictionaryUtility.FindDuplicateGroups(dictionary);
            }
            catch (Exception exception)
            {
                // キー型のユーザー実装（Equals / GetHashCode）が投げた例外。インスペクタ全体は落とさず報告する
                state.Results.Add(new ValidationMessage(
                    ValidationSeverity.Warning,
                    $"{label} のキーの比較中に例外が発生したため、重複を確認できません: {exception.Message}"));
                return;
            }

            foreach (List<int> indices in duplicates)
            {
                SerializedProperty key = dictionary.GetArrayElementAtIndex(indices[0])
                    .FindPropertyRelative(DictionaryUtility.KEY_NAME);

                state.Results.Add(new ValidationMessage(
                    ValidationSeverity.Error,
                    $"{label} のキー {DictionaryUtility.FormatKey(key, indices[0])} が {indices.Count} 件重複しています。"
                    + "実行時は最初の要素だけが使われます。"));
            }
        }

        private static void RunRules(
            SerializedProperty property,
            FieldInfo attributeSource,
            WalkState state,
            bool isInsideExpandedParent,
            CollectionRole role,
            Type valueType)
        {
            if (attributeSource == null) return;

            string label = BuildLabel(property, state);
            foreach (object candidate in attributeSource.GetCustomAttributes(false))
            {
                Attribute attribute = candidate as Attribute;
                if (attribute == null) continue;

                Type attributeType = attribute.GetType();

                IValidationRule rule = ExtensionRegistry.FindRule(attributeType);
                if (rule != null)
                {
                    rule.Validate(new ValidationContext(
                        property, attributeSource, attribute, label, state.Results, role, valueType));
                }

                // 要素はコンテナと同じ属性を引き継いでいるだけなので、コンテナ側で1回だけ報告する
                if (role == CollectionRole.Element
                    || role == CollectionRole.DictionaryKey
                    || role == CollectionRole.DictionaryValue)
                {
                    continue;
                }

                ReportUnsupportedNesting(attributeType, attributeSource, label, state, isInsideExpandedParent);
            }
        }

        /// <summary>
        /// 自前展開できなかったネスト（カスタム PropertyDrawer 持ち / [SerializeReference] / コレクションの要素 /
        /// 深さ超過）の中にある装飾属性は表示に反映されない。「付けたつもりで効いていない」状態を作らないよう明示する。
        /// 親が自前展開される場合（isInsideExpandedParent）は、装飾は通常どおり反映されるので警告しない。
        /// </summary>
        private static void ReportUnsupportedNesting(
            Type attributeType,
            FieldInfo attributeSource,
            string label,
            WalkState state,
            bool isInsideExpandedParent)
        {
            // 起点のフィールドの装飾は、そのフィールドの行に効いている
            if (attributeSource == state.RootField) return;
            if (isInsideExpandedParent) return;

            IPropertyDecorator decorator = ExtensionRegistry.FindDecorator(attributeType);
            if (decorator == null || decorator is ISupplementaryDecorator) return;

            state.Results.Add(new ValidationMessage(
                ValidationSeverity.Warning,
                $"{label} の [{TrimAttributeSuffix(attributeType.Name)}] は、配下の装飾として表示に反映されません。"));
        }

        private static string BuildLabel(SerializedProperty property, WalkState state)
        {
            string path = property.propertyPath;

            // Dictionary の要素の配下なら、キーで書いたラベルを起点にする
            if (state.TryFindRegisteredLabel(path, out string basePath, out string baseLabel))
            {
                return baseLabel + PropertyPathUtility.ToRelativePath(basePath, path);
            }

            string relativePath = PropertyPathUtility.ToRelativePath(state.RootPath, path);
            return relativePath.Length == 0 ? state.RootLabel : state.RootLabel + relativePath;
        }

        private static string TrimAttributeSuffix(string typeName)
            => typeName.EndsWith(ATTRIBUTE_SUFFIX, StringComparison.Ordinal)
                ? typeName.Substring(0, typeName.Length - ATTRIBUTE_SUFFIX.Length)
                : typeName;

        /// <summary>
        /// 同一メッセージをまとめる。int の配列に [Required] を付けた場合など、
        /// 要素ごとに同じ警告が並ぶのを防ぐ。
        /// </summary>
        private static void RemoveDuplicates(List<ValidationMessage> messages)
        {
            for (int i = messages.Count - 1; i > 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (!messages[i].Equals(messages[j])) continue;

                    messages.RemoveAt(i);
                    break;
                }
            }
        }

        private sealed class WalkState
        {
            private readonly Predicate<string> _hasOwnRow;

            // Dictionary の要素のキー・値のパスと、それを指すラベル。入れ子なら内側ほど後ろに積まれる
            private readonly List<(string Path, string Label)> _registeredLabels = new List<(string, string)>();

            public string RootPath { get; }
            public string RootLabel { get; }
            public FieldInfo RootField { get; }
            public List<ValidationMessage> Results { get; }

            public WalkState(
                string rootPath,
                string rootLabel,
                FieldInfo rootField,
                List<ValidationMessage> results,
                Predicate<string> hasOwnRow)
            {
                RootPath = rootPath;
                RootLabel = rootLabel;
                RootField = rootField;
                Results = results;
                _hasOwnRow = hasOwnRow;
            }

            public bool HasOwnRow(string propertyPath) => _hasOwnRow != null && _hasOwnRow(propertyPath);

            public void RegisterLabel(string propertyPath, string label) => _registeredLabels.Add((propertyPath, label));

            /// <summary>propertyPath 自身か、その祖先として登録されたうち最も深いものを探す。</summary>
            public bool TryFindRegisteredLabel(string propertyPath, out string basePath, out string label)
            {
                for (int i = _registeredLabels.Count - 1; i >= 0; i--)
                {
                    string candidate = _registeredLabels[i].Path;
                    bool matches = string.Equals(propertyPath, candidate, StringComparison.Ordinal)
                        || (propertyPath.StartsWith(candidate, StringComparison.Ordinal)
                            && propertyPath.Length > candidate.Length
                            && propertyPath[candidate.Length] == '.');

                    if (!matches) continue;

                    basePath = candidate;
                    label = _registeredLabels[i].Label;
                    return true;
                }

                basePath = null;
                label = null;
                return false;
            }
        }
    }
}
