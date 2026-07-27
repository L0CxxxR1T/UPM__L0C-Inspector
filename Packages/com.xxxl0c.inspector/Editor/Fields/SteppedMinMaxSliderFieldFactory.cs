using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [SteppedMinMaxSlider] のフィールドファクトリ。min / max フィールドを持つ値を
    /// MinMaxSlider + 左右の FloatField で表示する。
    /// </summary>
    public sealed class SteppedMinMaxSliderFieldFactory : IFieldFactory
    {
        public Type AttributeType => typeof(SteppedMinMaxSliderAttribute);

        public FieldFactoryResult Create(FieldFactoryContext context)
        {
            // このリポジトリの命名規約（_camelCase）を第一候補にし、旧ライブラリの契約（min/max）は
            // 後方互換のフォールバックとして残す
            SerializedProperty minProperty = FindChild(context.Property, "_min", "min");
            SerializedProperty maxProperty = FindChild(context.Property, "_max", "max");

            if (minProperty == null || maxProperty == null)
            {
                return FieldFactoryResult.Decline(
                    "[SteppedMinMaxSlider] は _min / _max（または min / max）フィールドを持つ型にのみ使えます。");
            }

            if (minProperty.propertyType != SerializedPropertyType.Float
                || maxProperty.propertyType != SerializedPropertyType.Float)
            {
                return FieldFactoryResult.Decline("[SteppedMinMaxSlider] の min / max は float 型である必要があります。");
            }

            SteppedMinMaxSliderAttribute attribute = (SteppedMinMaxSliderAttribute)context.Attribute;

            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            Label label = new Label(context.Property.displayName);
            label.AddToClassList(InspectorClassNames.AFFIX_LABEL);

            FloatField minField = new FloatField { isDelayed = true };
            minField.style.width = 48f;
            minField.BindProperty(minProperty);

            MinMaxSlider slider = new MinMaxSlider(attribute.Min, attribute.Max, attribute.Min, attribute.Max)
            {
                style = { flexGrow = 1 },
                value = new Vector2(minProperty.floatValue, maxProperty.floatValue)
            };

            FloatField maxField = new FloatField { isDelayed = true };
            maxField.style.width = 48f;
            maxField.BindProperty(maxProperty);

            bool syncing = false;

            void SyncFromFields()
            {
                if (syncing) return;
                syncing = true;
                slider.SetValueWithoutNotify(new Vector2(minField.value, maxField.value));
                syncing = false;
            }

            minField.RegisterValueChangedCallback(changeEvent =>
            {
                float snapped = SteppedValueUtility.Snap(changeEvent.newValue, attribute.Min, attribute.Max, attribute.Step);
                snapped = Mathf.Min(snapped, maxField.value);
                if (!Mathf.Approximately(snapped, changeEvent.newValue)) minField.value = snapped;
                else SyncFromFields();
            });

            maxField.RegisterValueChangedCallback(changeEvent =>
            {
                float snapped = SteppedValueUtility.Snap(changeEvent.newValue, attribute.Min, attribute.Max, attribute.Step);
                snapped = Mathf.Max(snapped, minField.value);
                if (!Mathf.Approximately(snapped, changeEvent.newValue)) maxField.value = snapped;
                else SyncFromFields();
            });

            slider.RegisterValueChangedCallback(changeEvent =>
            {
                if (syncing) return;

                float snappedMin = SteppedValueUtility.Snap(changeEvent.newValue.x, attribute.Min, attribute.Max, attribute.Step);
                float snappedMax = SteppedValueUtility.Snap(changeEvent.newValue.y, attribute.Min, attribute.Max, attribute.Step);

                // FloatField への代入が BindProperty 経由でプロパティに書き戻る
                minField.value = snappedMin;
                maxField.value = snappedMax;
                slider.SetValueWithoutNotify(new Vector2(snappedMin, snappedMax));
            });

            container.Add(label);
            container.Add(minField);
            container.Add(slider);
            container.Add(maxField);

            return FieldFactoryResult.Accept(container);
        }

        private static SerializedProperty FindChild(SerializedProperty property, string primaryName, string fallbackName)
            => property.FindPropertyRelative(primaryName) ?? property.FindPropertyRelative(fallbackName);
    }
}
