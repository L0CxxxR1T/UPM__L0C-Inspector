using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[EnableIf] の編集可否判定。</summary>
    public sealed class EnableIfRule : IEnabledRule
    {
        public Type AttributeType => typeof(EnableIfAttribute);

        public bool IsEnabled(VisibilityContext context)
        {
            EnableIfAttribute attribute = (EnableIfAttribute)context.Attribute;

            if (ConditionEvaluator.TryEvaluate(
                    context.Property, attribute.FieldName, attribute.ExpectedValue,
                    out bool matched, out string reason))
            {
                return matched;
            }

            // 評価できないときは編集を止めない。止めると設定もできず原因も見えなくなる
            context.Report(ValidationSeverity.Warning, $"[EnableIf] {reason}");
            return true;
        }
    }
}
