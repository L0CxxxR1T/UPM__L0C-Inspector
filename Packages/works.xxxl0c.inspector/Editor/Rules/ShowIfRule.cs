using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[ShowIf] の可視性判定。</summary>
    public sealed class ShowIfRule : IVisibilityRule
    {
        public Type AttributeType => typeof(ShowIfAttribute);

        public bool IsVisible(VisibilityContext context)
        {
            ShowIfAttribute attribute = (ShowIfAttribute)context.Attribute;

            if (ConditionEvaluator.TryEvaluate(
                    context.Property, attribute.FieldName, attribute.ExpectedValue,
                    out bool matched, out string reason))
            {
                return matched;
            }

            // 評価できないときは隠さない。隠すと設定もできず原因も見えなくなる
            context.Report(ValidationSeverity.Warning, $"[ShowIf] {reason}");
            return true;
        }
    }
}
