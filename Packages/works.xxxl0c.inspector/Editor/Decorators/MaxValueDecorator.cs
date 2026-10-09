using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[MaxValue] の装飾。インスペクタで上限を上回る値が入ったら上限に揃える。</summary>
    public sealed class MaxValueDecorator : IPropertyDecorator, ISupplementaryDecorator
    {
        public Type AttributeType => typeof(MaxValueAttribute);

        public int Order => 50;

        public void Decorate(DecorationContext context)
            => NumericBoundsValidation.TrackAndClamp(context, ((MaxValueAttribute)context.Attribute).Max, false);
    }
}
