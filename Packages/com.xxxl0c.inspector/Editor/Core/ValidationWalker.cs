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
            Visit(root, rootField, rootField.FieldType, state, 0, isInsideExpandedParent: true);
            RemoveDuplicates(results);
        }

        /// <param name="attributeSource">property に適用される属性の取得元。配列要素は親フィールドを引き継ぐ。</param>
        /// <param name="valueType">property の値の型。子フィールドを解決するのに使う。</param>
        /// <param name="isInsideExpandedParent">
        /// property の直近の親クラスが自前展開される対象か。
        /// true なら property の装飾属性はインスペクタで実際に反映されるので「反映されない」警告を出さない。
        /// インスペクタの `hasOwnRow` に頼らず独立に判定することで、一括チェックでも同じ結果になる。
        /// </param>
        private static void Visit(
            SerializedProperty property,
            FieldInfo attributeSource,
            Type valueType,
            WalkState state,
            int depth,
            bool isInsideExpandedParent)
        {
            if (depth > MAX_DEPTH) return;

            // コレクションのコンテナ自体ではルールを実行しない。
            // 要素側が親フィールドの属性を引き継いで評価するため、空リストは何も報告されない
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                VisitCollection(property, attributeSource, valueType, state, depth, isInsideExpandedParent);
                return;
            }

            RunRules(property, attributeSource, state, isInsideExpandedParent);

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
            // InspectorBuilder.CanExpand と同じ基準（[SerializeReference] は対象外、Drawer 持ちは対象外、深さ上限）
            bool childrenExpandable = property.propertyType != SerializedPropertyType.ManagedReference
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

                Visit(child, childField, childField.FieldType, state, depth + 1, childrenExpandable);
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
            Type elementType = SerializedFieldUtility.GetCollectionElementType(valueType);
            if (elementType == null) return;

            bool sourceHasAttribute = InspectedTypeCache.HasHandledAttribute(attributeSource);
            bool elementHasAttribute = InspectedTypeCache.ContainsHandledAttributes(elementType);
            if (!sourceHasAttribute && !elementHasAttribute) return;

            for (int i = 0; i < property.arraySize; i++)
            {
                Visit(
                    property.GetArrayElementAtIndex(i), attributeSource, elementType, state, depth + 1,
                    isInsideExpandedParent);
            }
        }

        private static void RunRules(
            SerializedProperty property,
            FieldInfo attributeSource,
            WalkState state,
            bool isInsideExpandedParent)
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
                    rule.Validate(new ValidationContext(property, attributeSource, attribute, label, state.Results));
                }

                ReportUnsupportedNesting(attributeType, attributeSource, label, state, isInsideExpandedParent);
            }
        }

        /// <summary>
        /// 自前展開できなかったネスト（カスタム PropertyDrawer 持ち / [SerializeReference] / 深さ超過）の
        /// 中にある装飾属性は表示に反映されない。「付けたつもりで効いていない」状態を作らないよう明示する。
        /// 親が自前展開される場合（isInsideExpandedParent）は、装飾は通常どおり反映されるので警告しない。
        /// </summary>
        private static void ReportUnsupportedNesting(
            Type attributeType,
            FieldInfo attributeSource,
            string label,
            WalkState state,
            bool isInsideExpandedParent)
        {
            // 配列要素は起点のフィールドを引き継いでいるだけなので対象外（装飾は親に効いている）
            if (attributeSource == state.RootField) return;
            if (isInsideExpandedParent) return;
            if (ExtensionRegistry.FindDecorator(attributeType) == null) return;

            state.Results.Add(new ValidationMessage(
                ValidationSeverity.Warning,
                $"{label} の [{TrimAttributeSuffix(attributeType.Name)}] は、配下の装飾として表示に反映されません。"));
        }

        private static string BuildLabel(SerializedProperty property, WalkState state)
        {
            string relativePath = PropertyPathUtility.ToRelativePath(state.RootPath, property.propertyPath);
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
        }
    }
}
