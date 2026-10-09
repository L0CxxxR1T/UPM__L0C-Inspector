using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// SerializedObject から VisualElement ツリーを組む静的ビルダー。
    /// フォールバック Editor だけでなく、個別 CustomEditor からも呼べる。
    /// 基底クラスの継承を強制しないのは、継承忘れが静かに壊れるのに対して
    /// 呼び忘れは「出ない」ので気づけるため。
    /// </summary>
    /// <example>
    /// <code>
    /// public override VisualElement CreateInspectorGUI()
    /// {
    ///     VisualElement root = InspectorBuilder.Build(this);
    ///     root.Add(new Button(Bake) { text = "焼き込む" });
    ///     return root;
    /// }
    /// </code>
    /// </example>
    public static class InspectorBuilder
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";
        private const string STYLE_SHEET_PATH =
            "Packages/works.xxxl0c.inspector/Editor/Styles/InspectorStyles.uss";
        private const string SCRIPT_PROPERTY_PATH = "m_Script";

        private static StyleSheet _styleSheet;
        private static bool _styleSheetErrorLogged;

        /// <summary>
        /// 属性を解釈したインスペクタのツリーを組んで返す。
        /// 扱う属性が1つも無いクラスは標準インスペクタをそのまま出す。
        /// </summary>
        public static VisualElement Build(UnityEditor.Editor editor)
        {
            SerializedObject serializedObject = editor.serializedObject;
            Type targetType = editor.target.GetType();

            VisualElement root = new VisualElement();
            root.AddToClassList(InspectorClassNames.ROOT);

            // 扱う属性もメソッドも1つも無いなら標準インスペクタに丸投げして即 return。
            // フォールバックが他人製アセットのコンポーネントを巻き込まないための必須ガード
            if (!InspectedTypeCache.ContainsHandledAttributes(targetType) && !ButtonMethodCache.HasButtonMethods(targetType))
            {
                InspectorElement.FillDefaultInspector(root, serializedObject, editor);
                return root;
            }

            ApplyStyleSheet(root);

            InspectorSession session = new InspectorSession(serializedObject);
            GroupTree groupTree = new GroupTree();
            GroupNode rootNode = groupTree.CreateRoot(string.Empty, root);
            session.SetGroupRoot(rootNode);

            BuildFields(CollectTopLevel(serializedObject), targetType, rootNode, groupTree, session, 0);
            ButtonSectionBuilder.Build(serializedObject, targetType, rootNode, groupTree);

            session.Refresh();
            root.TrackSerializedObjectValue(serializedObject, _ => session.Refresh());

            return root;
        }

        private static List<SerializedProperty> CollectTopLevel(SerializedObject serializedObject)
        {
            List<SerializedProperty> properties = new List<SerializedProperty>();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                properties.Add(iterator.Copy());
            }

            return properties;
        }

        private static void BuildFields(
            List<SerializedProperty> properties,
            Type ownerType,
            GroupNode parentNode,
            GroupTree groupTree,
            InspectorSession session,
            int depth)
        {
            foreach (SerializedProperty property in properties)
            {
                // Script フィールドは標準インスペクタと同じく編集不可で出す。グループには入れない
                if (depth == 0 && string.Equals(property.propertyPath, SCRIPT_PROPERTY_PATH, StringComparison.Ordinal))
                {
                    PropertyField scriptField = new PropertyField(property);
                    scriptField.SetEnabled(false);
                    parentNode.Content.Add(scriptField);
                    continue;
                }

                FieldInfo fieldInfo = SerializedFieldUtility.FindSerializedField(ownerType, property.name);
                if (fieldInfo == null)
                {
                    // C# 側に対応が無いプロパティ。トップレベルはそのまま出し、ネストでは触らない
                    if (depth == 0) parentNode.Content.Add(new PropertyField(property));
                    continue;
                }

                if (depth > 0 && fieldInfo.IsDefined(typeof(HideInInspector), false)) continue;

                BuildField(property, fieldInfo, parentNode, groupTree, session, depth);
            }
        }

        private static void BuildField(
            SerializedProperty property,
            FieldInfo fieldInfo,
            GroupNode parentNode,
            GroupTree groupTree,
            InspectorSession session,
            int depth)
        {
            bool expand = CanExpand(property, fieldInfo.FieldType, depth);

            // 属性の取得はここ1回だけ。更新パスは変更ごとに走るのでリフレクションを持ち込まない
            Attribute[] attributes = CollectAttributes(fieldInfo);

            VisualElement row = new VisualElement();
            row.AddToClassList(InspectorClassNames.ROW);

            VisualElement fieldElement;
            string declineReason = null;

            if (TryResolveFieldFactory(attributes, out Attribute factoryAttribute, out IFieldFactory factory))
            {
                FieldFactoryResult result = factory.Create(new FieldFactoryContext(property, fieldInfo, factoryAttribute));
                if (result.IsAccepted)
                {
                    // ファクトリが本体を丸ごと差し替えたら、その配下は factory の責任範囲。自前展開はしない
                    fieldElement = result.Element;
                    expand = false;
                }
                else
                {
                    declineReason = result.DeclineReason;
                    fieldElement = CreateDefaultField(property, fieldInfo, expand);
                }
            }
            else
            {
                fieldElement = CreateDefaultField(property, fieldInfo, expand);
            }

            row.Add(fieldElement);

            if (declineReason != null)
            {
                // 検証メッセージ（MESSAGES）とは別の永続領域に出す。更新パスで消されては困るため
                HelpBox notice = new HelpBox(declineReason, HelpBoxMessageType.Warning);
                notice.AddToClassList(InspectorClassNames.NOTICE);
                row.Add(notice);
            }

            VisualElement messageArea = new VisualElement();
            messageArea.AddToClassList(InspectorClassNames.MESSAGES);
            row.Add(messageArea);

            ApplyDecorators(property, fieldElement, row, fieldInfo, attributes);

            GroupNode ownerNode = ResolveGroup(parentNode, groupTree, attributes);
            InspectorRow inspectorRow = new InspectorRow(
                property.propertyPath, fieldInfo, attributes, property.displayName, row, fieldElement, messageArea);

            ownerNode.AddRow(inspectorRow);
            ownerNode.Content.Add(row);
            session.Register(inspectorRow);

            if (!expand) return;

            // 展開した中身は独立したルートノードにする。グループパスが外側と衝突しないようキーを分ける
            GroupNode nestedRoot = groupTree.CreateRoot(property.propertyPath, ((Foldout)fieldElement).contentContainer);
            ownerNode.AddChild(nestedRoot);

            List<SerializedProperty> children = new List<SerializedProperty>(
                SerializedFieldUtility.EnumerateDirectChildren(property));
            BuildFields(children, fieldInfo.FieldType, nestedRoot, groupTree, session, depth + 1);
        }

        private static VisualElement CreateDefaultField(SerializedProperty property, FieldInfo fieldInfo, bool expand)
        {
            if (expand) return CreateNestedFoldout(property);

            if (DictionaryUtility.TryGetKeyValueTypes(fieldInfo.FieldType, out Type keyType, out Type valueType))
            {
                return new DictionaryField(property, keyType, valueType);
            }

            return new PropertyField(property);
        }

        private static Foldout CreateNestedFoldout(SerializedProperty property)
        {
            Foldout foldout = new Foldout
            {
                text = property.displayName,
                value = true,
                viewDataKey = property.propertyPath
            };
            foldout.AddToClassList(InspectorClassNames.NESTED);
            return foldout;
        }

        /// <summary>
        /// ネストしたクラスを自前展開してよいかを判定する。
        /// カスタム PropertyDrawer を持つ型は展開しない（他人の描画を奪わない）。
        /// 判定できない場合も展開しない側に倒れるので、最悪でも PropertyField に丸投げに戻るだけ。
        /// </summary>
        private static bool CanExpand(SerializedProperty property, Type valueType, int depth)
        {
            if (depth >= InspectionLimits.MAX_NESTED_DEPTH) return false;
            if (property.propertyType != SerializedPropertyType.Generic) return false;
            if (property.isArray) return false;
            if (!property.hasVisibleChildren) return false;
            if (!InspectedTypeCache.ContainsHandledAttributes(valueType)) return false;

            return !PropertyDrawerRegistry.HasDrawer(valueType);
        }

        private static Attribute[] CollectAttributes(FieldInfo fieldInfo)
        {
            List<Attribute> attributes = new List<Attribute>();
            foreach (object candidate in fieldInfo.GetCustomAttributes(false))
            {
                if (candidate is Attribute attribute) attributes.Add(attribute);
            }

            return attributes.ToArray();
        }

        /// <summary>
        /// フィールド本体を差し替える属性を探す。装飾（IPropertyDecorator）と違い1フィールドに
        /// 1つしか適用できないので、複数見つかったら LogError で報告して型名順で先着を採用する。
        /// </summary>
        private static bool TryResolveFieldFactory(
            Attribute[] attributes, out Attribute matchedAttribute, out IFieldFactory factory)
        {
            List<(Attribute Attribute, IFieldFactory Factory)> candidates = new List<(Attribute, IFieldFactory)>();
            foreach (Attribute attribute in attributes)
            {
                IFieldFactory candidate = ExtensionRegistry.FindFieldFactory(attribute.GetType());
                if (candidate != null) candidates.Add((attribute, candidate));
            }

            if (candidates.Count == 0)
            {
                matchedAttribute = null;
                factory = null;
                return false;
            }

            if (candidates.Count > 1)
            {
                candidates.Sort((left, right) =>
                    string.Compare(left.Attribute.GetType().Name, right.Attribute.GetType().Name, StringComparison.Ordinal));

                List<string> names = candidates.ConvertAll(candidate => candidate.Attribute.GetType().Name);
                Debug.LogError(
                    $"{LOG_PREFIX}同じフィールドに本体を差し替える属性が複数付いています（{string.Join(", ", names)}）。"
                    + $"{names[0]} を使用します。");
            }

            matchedAttribute = candidates[0].Attribute;
            factory = candidates[0].Factory;
            return true;
        }

        private static GroupNode ResolveGroup(GroupNode parentNode, GroupTree groupTree, Attribute[] attributes)
        {
            foreach (Attribute attribute in attributes)
            {
                IGroupContainerFactory factory = ExtensionRegistry.FindGroupFactory(attribute.GetType());
                if (factory == null) continue;

                return groupTree.Resolve(parentNode, factory.GetPath(attribute), factory);
            }

            return parentNode;
        }

        /// <summary>
        /// フィールドに付いた装飾属性を全部適用する。
        /// PropertyDrawer と違い1フィールドに何個付いていても効くのが、この設計の要点。
        /// </summary>
        private static void ApplyDecorators(
            SerializedProperty property,
            VisualElement fieldElement,
            VisualElement row,
            FieldInfo fieldInfo,
            Attribute[] attributes)
        {
            List<PendingDecoration> pending = new List<PendingDecoration>();
            foreach (Attribute attribute in attributes)
            {
                IPropertyDecorator decorator = ExtensionRegistry.FindDecorator(attribute.GetType());
                if (decorator == null) continue;

                pending.Add(new PendingDecoration(decorator, attribute));
            }

            if (pending.Count == 0) return;

            // List.Sort は不安定なので、Order が同じときは型名で決着させて順序を固定する
            pending.Sort((left, right) =>
            {
                int byOrder = left.Decorator.Order.CompareTo(right.Decorator.Order);
                return byOrder != 0
                    ? byOrder
                    : string.Compare(left.Decorator.GetType().Name, right.Decorator.GetType().Name, StringComparison.Ordinal);
            });

            foreach (PendingDecoration entry in pending)
            {
                entry.Decorator.Decorate(
                    new DecorationContext(property, fieldElement, row, fieldInfo, entry.Attribute));
            }
        }

        private static void ApplyStyleSheet(VisualElement root)
        {
            if (_styleSheet == null)
            {
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_PATH);
            }

            if (_styleSheet == null)
            {
                if (_styleSheetErrorLogged) return;

                _styleSheetErrorLogged = true;
                Debug.LogError($"{LOG_PREFIX}スタイルシートが見つかりません: {STYLE_SHEET_PATH}");
                return;
            }

            root.styleSheets.Add(_styleSheet);
        }

        private readonly struct PendingDecoration
        {
            public IPropertyDecorator Decorator { get; }
            public Attribute Attribute { get; }

            public PendingDecoration(IPropertyDecorator decorator, Attribute attribute)
            {
                Decorator = decorator;
                Attribute = attribute;
            }
        }
    }
}
