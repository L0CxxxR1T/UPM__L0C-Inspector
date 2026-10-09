using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// Optional&lt;T&gt; の Drawer。値の欄と、値の有無を切り替えるチェックボックスを1行に並べる。
    /// 値が無いときは値の欄を表示のみにする（折りたたみの開閉はできる）。
    /// </summary>
    [CustomPropertyDrawer(typeof(Optional<>))]
    internal sealed class OptionalDrawer : PropertyDrawer
    {
        private const string HAS_VALUE_FIELD = "_hasValue";
        private const string VALUE_FIELD = "_value";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty hasValue = property.FindPropertyRelative(HAS_VALUE_FIELD);
            SerializedProperty value = property.FindPropertyRelative(VALUE_FIELD);

            VisualElement row = new VisualElement();
            row.AddToClassList(InspectorClassNames.OPTIONAL);

            // 属性の無いクラスでは標準インスペクタの中で描かれ、スタイルシートが当たっていない
            InspectorBuilder.ApplyStyleSheet(row);

            PropertyField valueField = new PropertyField(value, preferredLabel);
            valueField.AddToClassList(InspectorClassNames.OPTIONAL_VALUE);

            Toggle toggle = new Toggle { tooltip = "チェックを入れると値が有効になります。" };
            toggle.AddToClassList(InspectorClassNames.OPTIONAL_TOGGLE);
            toggle.BindProperty(hasValue);

            void Apply(SerializedProperty current)
                => ReadOnlyGuard.SetLocked(valueField, typeof(OptionalDrawer), !current.boolValue && !current.hasMultipleDifferentValues);

            toggle.TrackPropertyValue(hasValue, Apply);
            Apply(hasValue);

            row.Add(valueField);
            row.Add(toggle);
            return row;
        }
    }
}
