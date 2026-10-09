using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[MaxValue] の検証。上限を上回る値が保存されていれば Error にする。</summary>
    public sealed class MaxValueRule : IValidationRule
    {
        public Type AttributeType => typeof(MaxValueAttribute);

        public void Validate(ValidationContext context)
            => NumericBoundsValidation.Validate(context, ((MaxValueAttribute)context.Attribute).Max, false, "MaxValue");
    }
}
