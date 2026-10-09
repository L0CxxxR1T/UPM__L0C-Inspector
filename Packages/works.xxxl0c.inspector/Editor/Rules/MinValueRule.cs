using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[MinValue] の検証。下限を下回る値が保存されていれば Error にする。</summary>
    public sealed class MinValueRule : IValidationRule
    {
        public Type AttributeType => typeof(MinValueAttribute);

        public void Validate(ValidationContext context)
            => NumericBoundsValidation.Validate(context, ((MinValueAttribute)context.Attribute).Min, true, "MinValue");
    }
}
