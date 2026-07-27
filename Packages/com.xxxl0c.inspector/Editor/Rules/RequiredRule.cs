using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [Required] の検証。渡された1プロパティだけを見る。
    /// 配列要素への展開は走査層（ValidationWalker）が済ませているので、ここでループは持たない。
    /// </summary>
    public sealed class RequiredRule : IValidationRule
    {
        public Type AttributeType => typeof(RequiredAttribute);

        public void Validate(ValidationContext context)
        {
            RequiredAttribute attribute = (RequiredAttribute)context.Attribute;
            SerializedProperty property = context.Property;

            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    if (property.objectReferenceValue == null) ReportMissing(context, attribute);
                    break;

                case SerializedPropertyType.ManagedReference:
                    if (string.IsNullOrEmpty(property.managedReferenceFullTypename)) ReportMissing(context, attribute);
                    break;

                default:
                    // 「未設定」の概念が無い型。黙って無視すると守られていないことに気づけない
                    context.Report(
                        ValidationSeverity.Warning,
                        $"[Required] は {property.propertyType} 型には効果がありません。属性を外してください。");
                    break;
            }
        }

        private static void ReportMissing(ValidationContext context, RequiredAttribute attribute)
        {
            string message = string.IsNullOrEmpty(attribute.Message)
                ? $"{context.Label} が未設定です。"
                : attribute.Message;

            context.Report(ValidationSeverity.Error, message);
        }
    }
}
