using System;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[Suffix] の装飾。本体の後ろにテキストを添える。詳細は <see cref="PrefixDecorator"/> を参照。</summary>
    public sealed class SuffixDecorator : IPropertyDecorator
    {
        public Type AttributeType => typeof(SuffixAttribute);

        public int Order => 10;

        public void Decorate(DecorationContext context)
        {
            SuffixAttribute attribute = (SuffixAttribute)context.Attribute;
            VisualElement affixRow = AffixRowUtility.GetOrCreateRow(context);
            affixRow.Add(AffixRowUtility.CreateAffixLabel(attribute.Text));
        }
    }
}
