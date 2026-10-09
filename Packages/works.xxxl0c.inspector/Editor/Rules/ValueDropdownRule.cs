using System;
using System.Collections.Generic;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [ValueDropdown] の検証。候補を読めない誤用と、候補に無い値（候補の変更で取り残された値など）を Warning にする。
    /// </summary>
    public sealed class ValueDropdownRule : IValidationRule
    {
        public Type AttributeType => typeof(ValueDropdownAttribute);

        public void Validate(ValidationContext context)
        {
            // 配列・List は ValueDropdownFieldFactory が未対応を通知する
            if (context.Role != CollectionRole.None) return;

            ValueDropdownAttribute attribute = (ValueDropdownAttribute)context.Attribute;
            if (!ValueDropdownSource.TryGetEntries(
                    context.FieldInfo.DeclaringType, context.ValueType, attribute,
                    out List<ValueDropdownEntry> entries, out string reason))
            {
                context.Report(ValidationSeverity.Warning, $"[ValueDropdown] {reason}");
                return;
            }

            if (ValueDropdownSource.IndexOf(entries, context.Property) >= 0) return;

            context.Report(
                ValidationSeverity.Warning,
                $"{context.Label} の値 {ValueDropdownSource.FormatCurrent(context.Property)} は [ValueDropdown] の候補にありません。");
        }
    }
}
