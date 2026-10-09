using System;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[SceneObjectsOnly] の検証。アセットを指していれば Error にする。</summary>
    public sealed class SceneObjectsOnlyRule : IValidationRule
    {
        private const string ATTRIBUTE_NAME = "SceneObjectsOnly";

        public Type AttributeType => typeof(SceneObjectsOnlyAttribute);

        public void Validate(ValidationContext context)
        {
            Object target = ObjectReferenceValidation.GetTarget(context, ATTRIBUTE_NAME);
            if (target == null) return;

            Object owner = context.Property.serializedObject.targetObject;
            if (ObjectReferenceValidation.IsSceneObject(target, owner)) return;

            context.Report(
                ValidationSeverity.Error,
                $"{context.Label} にはシーン上のオブジェクトを指定してください（アセット '{target.name}' が入っています）。");
        }
    }
}
