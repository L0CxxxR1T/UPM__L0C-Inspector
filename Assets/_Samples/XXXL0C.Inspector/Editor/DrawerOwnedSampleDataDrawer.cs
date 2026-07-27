using UnityEditor;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Samples.Editor
{
    /// <summary>
    /// 型に対する描画は PropertyDrawer の担当（属性による装飾は中央 Editor の担当）。
    /// この Drawer があることで DrawerOwnedSampleData は自前展開の対象外になる。
    /// </summary>
    [CustomPropertyDrawer(typeof(DrawerOwnedSampleData))]
    public sealed class DrawerOwnedSampleDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Foldout foldout = new Foldout
            {
                text = $"{property.displayName}（専用 Drawer で描画）",
                value = true,
                viewDataKey = property.propertyPath
            };

            foreach (SerializedProperty child in EnumerateChildren(property))
            {
                foldout.Add(new UnityEditor.UIElements.PropertyField(child));
            }

            return foldout;
        }

        private static System.Collections.Generic.IEnumerable<SerializedProperty> EnumerateChildren(
            SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            int childDepth = property.depth + 1;
            bool enterChildren = true;

            while (iterator.Next(enterChildren))
            {
                enterChildren = false;
                if (SerializedProperty.EqualContents(iterator, end)) break;
                if (iterator.depth < childDepth) break;

                yield return iterator.Copy();
            }
        }
    }
}
