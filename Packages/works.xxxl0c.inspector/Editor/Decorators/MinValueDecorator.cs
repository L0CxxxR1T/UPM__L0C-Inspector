using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[MinValue] の装飾。インスペクタで下限を下回る値が入ったら下限に揃える。</summary>
    public sealed class MinValueDecorator : IPropertyDecorator, ISupplementaryDecorator
    {
        public Type AttributeType => typeof(MinValueAttribute);

        public int Order => 50;

        public void Decorate(DecorationContext context)
            => NumericBoundsValidation.TrackAndClamp(context, ((MinValueAttribute)context.Attribute).Min, true);
    }
}
