using System;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[Label] の装飾。表示名を差し替える。</summary>
    public sealed class LabelDecorator : IPropertyDecorator
    {
        public Type AttributeType => typeof(LabelAttribute);

        public int Order => 0;

        public void Decorate(DecorationContext context)
        {
            LabelAttribute attribute = (LabelAttribute)context.Attribute;

            if (context.AsPropertyField != null)
            {
                context.AsPropertyField.label = attribute.DisplayName;
            }
            else if (context.Field is Foldout foldout)
            {
                // 自前展開されたネストクラスでは本体が Foldout になる
                foldout.text = attribute.DisplayName;
            }
            else if (context.FactoryLabel != null)
            {
                // IFieldFactory が差し替えた本体
                context.FactoryLabel.text = attribute.DisplayName;
            }
        }
    }
}
