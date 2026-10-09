using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// SerializableType の Drawer。フィールドに付いた [TypeFilter(typeof(基底型))] で候補を絞ったドロップダウンを出す。
    /// 型の Drawer なので配列・List の要素にも効く。候補に無い名前（クラスの改名・削除）は TypeFilterRule が報告する。
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableType))]
    internal sealed class SerializableTypeDrawer : PropertyDrawer
    {
        private const string TYPE_NAME_FIELD = "_typeName";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty nameProperty = property.FindPropertyRelative(TYPE_NAME_FIELD);
            TypeFilterAttribute filter = fieldInfo.GetCustomAttribute<TypeFilterAttribute>();

            // 候補を絞れないと全型が並んで選べないので、名前の表示だけにする
            if (filter?.BaseType == null) return CreateNameOnly(property, nameProperty, filter == null);

            IReadOnlyList<Type> candidates = TypeCandidates.ForSerializableType(filter.BaseType);
            List<string> choices = new List<string> { TypeCandidates.NONE_CHOICE };
            choices.AddRange(TypeCandidates.BuildDisplayNames(candidates));

            DropdownField dropdown = new DropdownField(GetLabel(property), choices, 0);
            dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);

            SerializedObject serializedObject = property.serializedObject;
            string namePath = nameProperty.propertyPath;

            void Sync(SerializedProperty current)
            {
                dropdown.showMixedValue = current.hasMultipleDifferentValues;
                dropdown.SetValueWithoutNotify(ToChoice(current.stringValue, candidates, choices));
            }

            dropdown.RegisterValueChangedCallback(changeEvent =>
            {
                int index = choices.IndexOf(changeEvent.newValue);
                if (index < 0) return;

                SerializedProperty current = serializedObject.FindProperty(namePath);
                current.stringValue = index == 0 ? string.Empty : candidates[index - 1].FullName;
                serializedObject.ApplyModifiedProperties();
            });

            dropdown.TrackPropertyValue(nameProperty, Sync);
            Sync(nameProperty);

            return dropdown;
        }

        private VisualElement CreateNameOnly(SerializedProperty property, SerializedProperty nameProperty, bool missingFilter)
        {
            VisualElement root = new VisualElement();

            TextField text = new TextField(GetLabel(property)) { isReadOnly = true };
            text.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            text.BindProperty(nameProperty);
            root.Add(text);

            // 基底型の指定漏れは TypeFilterRule が検証層で報告する。属性が無い場合だけここで案内する
            if (missingFilter)
            {
                root.Add(new HelpBox(
                    "[TypeFilter(typeof(基底型))] を付けると、型をドロップダウンで選べます。", HelpBoxMessageType.Info));
            }

            return root;
        }

        /// <summary>
        /// 配列要素では "Element 0" を使う。Unity は要素の先頭の string フィールドを見出しにする
        /// （displayName も同じ）ので、そのままだと型名がラベルに出てしまう。
        /// </summary>
        private string GetLabel(SerializedProperty property)
        {
            string path = property.propertyPath;
            if (!path.EndsWith("]", StringComparison.Ordinal)) return preferredLabel;

            int open = path.LastIndexOf('[');
            return $"Element {path.Substring(open + 1, path.Length - open - 2)}";
        }

        private static string ToChoice(string typeName, IReadOnlyList<Type> candidates, List<string> choices)
        {
            if (string.IsNullOrEmpty(typeName)) return TypeCandidates.NONE_CHOICE;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (string.Equals(candidates[i].FullName, typeName, StringComparison.Ordinal)) return choices[i + 1];
            }

            return $"{typeName}（見つかりません）";
        }
    }
}
