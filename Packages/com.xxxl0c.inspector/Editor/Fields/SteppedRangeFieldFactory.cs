using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[SteppedRange] のフィールドファクトリ。float フィールドをステップ丸め付きスライダーにする。</summary>
    public sealed class SteppedRangeFieldFactory : IFieldFactory
    {
        public Type AttributeType => typeof(SteppedRangeAttribute);

        public FieldFactoryResult Create(FieldFactoryContext context)
        {
            if (context.Property.propertyType != SerializedPropertyType.Float)
            {
                return FieldFactoryResult.Decline(
                    $"[SteppedRange] は Float 型にのみ使えます（{context.Property.propertyType} には使えません）。");
            }

            SteppedRangeAttribute attribute = (SteppedRangeAttribute)context.Attribute;

            Slider slider = new Slider(context.Property.displayName, attribute.Min, attribute.Max)
            {
                showInputField = true
            };
            slider.BindProperty(context.Property);

            // BindProperty は生の値をそのまま書き戻すので、丸めは変更イベント側で上書きする形で行う
            slider.RegisterValueChangedCallback(changeEvent =>
            {
                float snapped = SteppedValueUtility.Snap(changeEvent.newValue, attribute.Min, attribute.Max, attribute.Step);
                if (!Mathf.Approximately(snapped, changeEvent.newValue))
                {
                    slider.value = snapped;
                }
            });

            return FieldFactoryResult.Accept(slider);
        }
    }
}
