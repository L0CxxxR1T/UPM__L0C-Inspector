using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[NotEmpty] の検証。空文字・要素が0個のコレクションを Error にする。</summary>
    public sealed class NotEmptyRule : IValidationRule
    {
        public Type AttributeType => typeof(NotEmptyAttribute);

        public void Validate(ValidationContext context)
        {
            NotEmptyAttribute attribute = (NotEmptyAttribute)context.Attribute;
            SerializedProperty property = context.Property;

            switch (context.Role)
            {
                case CollectionRole.Collection:
                case CollectionRole.Dictionary:
                    if (property.arraySize == 0) ReportEmpty(context, attribute);
                    return;

                case CollectionRole.None:
                    if (property.propertyType == SerializedPropertyType.String)
                    {
                        if (string.IsNullOrEmpty(property.stringValue)) ReportEmpty(context, attribute);
                        return;
                    }

                    context.Report(
                        ValidationSeverity.Warning,
                        $"[NotEmpty] は {property.propertyType} 型には効果がありません。string / 配列 / List / Dictionary にのみ使えます。");
                    return;

                default:
                    // 要素の中身はコンテナ単位の判定の対象外
                    return;
            }
        }

        private static void ReportEmpty(ValidationContext context, NotEmptyAttribute attribute)
        {
            string message = string.IsNullOrEmpty(attribute.Message) ? $"{context.Label} が空です。" : attribute.Message;
            context.Report(ValidationSeverity.Error, message);
        }
    }
}
