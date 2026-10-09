using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[DisableIf] の編集可否判定。[EnableIf] の反転。</summary>
    public sealed class DisableIfRule : IEnabledRule
    {
        public Type AttributeType => typeof(DisableIfAttribute);

        public bool IsEnabled(VisibilityContext context)
        {
            DisableIfAttribute attribute = (DisableIfAttribute)context.Attribute;

            if (ConditionEvaluator.TryEvaluate(
                    context.Property, attribute.FieldName, attribute.ExpectedValue,
                    out bool matched, out string reason))
            {
                return !matched;
            }

            // 評価できないときは編集を止めない。止めると設定もできず原因も見えなくなる
            context.Report(ValidationSeverity.Warning, $"[DisableIf] {reason}");
            return true;
        }
    }
}
