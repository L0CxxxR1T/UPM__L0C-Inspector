using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// SerializableType に付けた [TypeFilter] の検証。保存している型名が候補に無ければ Error にする。
    /// 型名で保存しているので、クラスの改名・削除や基底型の変更で黙って参照が切れるのを防ぐ。
    /// </summary>
    public sealed class TypeFilterRule : IValidationRule
    {
        private const string TYPE_NAME_FIELD = "_typeName";

        public Type AttributeType => typeof(TypeFilterAttribute);

        public void Validate(ValidationContext context)
        {
            if (context.ValueType != typeof(SerializableType)) return;
            if (context.Role != CollectionRole.None && context.Role != CollectionRole.Element) return;

            TypeFilterAttribute attribute = (TypeFilterAttribute)context.Attribute;
            if (attribute.BaseType == null)
            {
                context.Report(
                    ValidationSeverity.Warning,
                    "[TypeFilter] を SerializableType に使うときは typeof(基底型) を指定してください。");
                return;
            }

            SerializedProperty nameProperty = context.Property.FindPropertyRelative(TYPE_NAME_FIELD);
            if (nameProperty == null || string.IsNullOrEmpty(nameProperty.stringValue)) return;
            if (TypeCandidates.ContainsSerializableType(attribute.BaseType, nameProperty.stringValue)) return;

            context.Report(
                ValidationSeverity.Error,
                $"{context.Label} の型 '{nameProperty.stringValue}' は {attribute.BaseType.Name} の候補にありません。"
                + "クラスの改名・削除や、基底型の変更で参照が切れています。");
        }
    }
}
