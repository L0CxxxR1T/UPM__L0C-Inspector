using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[ChildGameObjectsOnly] の検証。持ち主の子孫以外の GameObject / Component を指していれば Error にする。</summary>
    public sealed class ChildGameObjectsOnlyRule : IValidationRule
    {
        private const string ATTRIBUTE_NAME = "ChildGameObjectsOnly";

        public Type AttributeType => typeof(ChildGameObjectsOnlyAttribute);

        public void Validate(ValidationContext context)
        {
            Object owner = context.Property.serializedObject.targetObject;
            if (!(owner is Component ownerComponent))
            {
                context.Report(
                    ValidationSeverity.Warning,
                    $"[{ATTRIBUTE_NAME}] は Component のフィールドにのみ使えます（{owner.GetType().Name} には効果がありません）。");
                return;
            }

            Object target = ObjectReferenceValidation.GetTarget(context, ATTRIBUTE_NAME);
            if (target == null) return;

            Transform targetTransform = target is GameObject gameObject
                ? gameObject.transform
                : (target as Component)?.transform;

            if (targetTransform == null)
            {
                context.Report(
                    ValidationSeverity.Warning,
                    $"[{ATTRIBUTE_NAME}] は GameObject / Component の参照にのみ使えます（{context.Label}）。");
                return;
            }

            ChildGameObjectsOnlyAttribute attribute = (ChildGameObjectsOnlyAttribute)context.Attribute;
            Transform ownerTransform = ownerComponent.transform;

            bool allowed = targetTransform == ownerTransform
                ? attribute.IncludeSelf
                : targetTransform.IsChildOf(ownerTransform);
            if (allowed) return;

            string scope = attribute.IncludeSelf ? "自身か子孫" : "子孫";
            context.Report(
                ValidationSeverity.Error,
                $"{context.Label} には '{ownerComponent.name}' の{scope}を指定してください（'{target.name}' は対象外です）。");
        }
    }
}
