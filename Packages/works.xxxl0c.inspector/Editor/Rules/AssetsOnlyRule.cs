using System;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[AssetsOnly] の検証。シーン上のオブジェクトを指していれば Error にする。</summary>
    public sealed class AssetsOnlyRule : IValidationRule
    {
        private const string ATTRIBUTE_NAME = "AssetsOnly";

        public Type AttributeType => typeof(AssetsOnlyAttribute);

        public void Validate(ValidationContext context)
        {
            Object target = ObjectReferenceValidation.GetTarget(context, ATTRIBUTE_NAME);
            if (target == null) return;

            Object owner = context.Property.serializedObject.targetObject;
            if (!ObjectReferenceValidation.IsSceneObject(target, owner)) return;

            context.Report(
                ValidationSeverity.Error,
                $"{context.Label} にはアセットを指定してください（シーン上のオブジェクト '{target.name}' が入っています）。");
        }
    }
}
