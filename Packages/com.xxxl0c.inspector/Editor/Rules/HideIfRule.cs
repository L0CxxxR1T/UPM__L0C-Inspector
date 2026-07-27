using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[HideIf] の可視性判定。[ShowIf] の反転。</summary>
    public sealed class HideIfRule : IVisibilityRule
    {
        public Type AttributeType => typeof(HideIfAttribute);

        public bool IsVisible(VisibilityContext context)
        {
            HideIfAttribute attribute = (HideIfAttribute)context.Attribute;

            if (ConditionEvaluator.TryEvaluate(
                    context.Property, attribute.FieldName, attribute.ExpectedValue,
                    out bool matched, out string reason))
            {
                return !matched;
            }

            // 評価できないときは隠さない。隠すと設定もできず原因も見えなくなる
            context.Report(ValidationSeverity.Warning, $"[HideIf] {reason}");
            return true;
        }
    }
}
