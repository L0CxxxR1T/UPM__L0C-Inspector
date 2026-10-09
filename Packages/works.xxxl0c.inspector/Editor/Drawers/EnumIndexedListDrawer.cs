using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// EnumIndexedList の Drawer。要素を enum のメンバー名を見出しにして並べる。
    /// 要素の追加・削除・並べ替えはできない（数と並びは enum が決める）。
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumIndexedList<,>))]
    internal sealed class EnumIndexedListDrawer : PropertyDrawer
    {
        private const string ITEMS_FIELD = "_items";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Foldout foldout = new Foldout { text = preferredLabel, viewDataKey = property.propertyPath };

            Type listType = GenericTypeUtility.FindClosed(fieldInfo.FieldType, typeof(EnumIndexedList<,>));
            if (listType == null)
            {
                foldout.Add(new HelpBox("EnumIndexedList の enum 型を特定できませんでした。", HelpBoxMessageType.Warning));
                return foldout;
            }

            List<string> names = GetMemberNames(listType.GetGenericArguments()[0]);
            SerializedProperty items = property.FindPropertyRelative(ITEMS_FIELD);

            int shown = Math.Min(names.Count, items.arraySize);
            for (int i = 0; i < shown; i++)
            {
                foldout.Add(new PropertyField(items.GetArrayElementAtIndex(i), names[i]));
            }

            // 読み込み時に埋めるので通常は起きない。起きたら黙って欠けた表示にしない
            if (items.arraySize < names.Count)
            {
                foldout.Add(new HelpBox(
                    $"要素数（{items.arraySize}）が enum のメンバー数（{names.Count}）より少なくなっています。",
                    HelpBoxMessageType.Warning));
            }

            return foldout;
        }

        /// <summary>実行時の EnumIndexedList と同じ並び（宣言順、同じ値の別名は最初のものだけ）で名前を返す。</summary>
        private static List<string> GetMemberNames(Type enumType)
        {
            List<string> names = new List<string>();
            HashSet<object> seen = new HashSet<object>();
            foreach (object value in Enum.GetValues(enumType))
            {
                if (seen.Add(value)) names.Add(ObjectNames.NicifyVariableName(Enum.GetName(enumType, value)));
            }

            return names;
        }
    }
}
