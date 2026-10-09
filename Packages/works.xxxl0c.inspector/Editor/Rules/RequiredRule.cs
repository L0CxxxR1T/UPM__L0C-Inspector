using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [Required] の検証。渡された1プロパティだけを見る。
    /// 配列要素・Dictionary の要素への展開は走査層（ValidationWalker）が済ませているので、ここでループは持たない。
    /// </summary>
    public sealed class RequiredRule : IValidationRule
    {
        private const string SERIALIZABLE_TYPE_NAME_FIELD = "_typeName";

        public Type AttributeType => typeof(RequiredAttribute);

        public void Validate(ValidationContext context)
        {
            RequiredAttribute attribute = (RequiredAttribute)context.Attribute;
            SerializedProperty property = context.Property;

            switch (context.Role)
            {
                case CollectionRole.Dictionary:
                    // 効果の有無はキーと値の型の組み合わせで決まるので、コンテナで1回だけ判定する
                    if (!DictionaryUtility.CanContainNull(context.ValueType))
                    {
                        context.Report(
                            ValidationSeverity.Warning,
                            "[Required] はキーにも値にも「未設定」の概念が無い Dictionary には効果がありません。属性を外してください。");
                    }
                    return;

                case CollectionRole.DictionaryKey:
                    if (DictionaryUtility.IsNullKey(property))
                    {
                        context.Report(
                            ValidationSeverity.Error,
                            string.IsNullOrEmpty(attribute.Message) ? $"{context.Label} のキーが未設定です。" : attribute.Message);
                    }
                    return;

                case CollectionRole.DictionaryValue:
                    // 未設定になり得ない型の値は、コンテナ側の判定に任せて黙る
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue == null)
                    {
                        ReportMissing(context, attribute);
                    }
                    return;
            }

            // SerializableType は型名が空なら未設定
            if (context.ValueType == typeof(SerializableType))
            {
                SerializedProperty typeName = property.FindPropertyRelative(SERIALIZABLE_TYPE_NAME_FIELD);
                if (typeName != null && string.IsNullOrEmpty(typeName.stringValue)) ReportMissing(context, attribute);
                return;
            }

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
