using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [InlineEditor] の装飾。Object 参照フィールドの下に、参照先のインスペクタをそのまま埋め込む。
    /// IMGUI 版の Editor キャッシュ管理は移植しない（UI Toolkit の InspectorElement のみ対応）。
    /// </summary>
    public sealed class InlineEditorDecorator : IPropertyDecorator
    {
        public Type AttributeType => typeof(InlineEditorAttribute);

        public int Order => 30;

        public void Decorate(DecorationContext context)
        {
            PropertyField field = context.AsPropertyField;
            if (field == null) return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
            {
                context.Row.Add(new HelpBox(
                    "[InlineEditor] は Object 参照フィールドにのみ使えます。", HelpBoxMessageType.Warning));
                return;
            }

            InlineEditorAttribute attribute = (InlineEditorAttribute)context.Attribute;
            VisualElement container = new VisualElement();
            container.AddToClassList(InspectorClassNames.INLINE_EDITOR);
            context.Row.Add(container);

            void Rebuild()
            {
                container.Clear();

                Object target = context.Property.objectReferenceValue;
                if (target == null) return;

                if (attribute.ShowOpenButton)
                {
                    container.Add(new Button(() => AssetDatabase.OpenAsset(target)) { text = "開く" });
                }

                InspectorElement inlineInspector = new InspectorElement(target);
                container.Add(inlineInspector);
            }

            field.RegisterValueChangeCallback(_ => Rebuild());
            Rebuild();
        }
    }
}
