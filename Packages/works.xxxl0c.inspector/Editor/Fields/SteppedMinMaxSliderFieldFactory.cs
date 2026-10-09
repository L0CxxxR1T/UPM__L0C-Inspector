using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [SteppedMinMaxSlider] のフィールドファクトリ。min / max フィールドを持つ値を
    /// MinMaxSlider + 左右の数値欄で表示する。min / max は float 同士か int 同士に対応する。
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

            bool isInteger = minProperty.propertyType == SerializedPropertyType.Integer;
            bool isNumeric = isInteger || minProperty.propertyType == SerializedPropertyType.Float;
            if (!isNumeric || minProperty.propertyType != maxProperty.propertyType)
            {
                return FieldFactoryResult.Decline(
                    "[SteppedMinMaxSlider] の min / max は float 同士か int 同士である必要があります。");
            }

            SteppedMinMaxSliderAttribute attribute = (SteppedMinMaxSliderAttribute)context.Attribute;
            string minPath = minProperty.propertyPath;
            string maxPath = maxProperty.propertyPath;

            float Snap(float value)
            {
                float snapped = SteppedValueUtility.Snap(value, attribute.Min, attribute.Max, attribute.Step);
                return isInteger ? Mathf.Round(snapped) : snapped;
            }

            ValueInput minInput = CreateInput(minProperty);
            ValueInput maxInput = CreateInput(maxProperty);

            MinMaxSlider slider = new MinMaxSlider(
                context.Property.displayName, ReadValue(minProperty), ReadValue(maxProperty), attribute.Min, attribute.Max);
            slider.AddToClassList(BaseField<Vector2>.alignedFieldUssClassName);

            // ラベル幅の揃えは BaseField 本体にしか効かないので、数値欄はスライダーの中に差し込む
            slider.Insert(slider.IndexOf(slider.labelElement) + 1, minInput.Element);
            slider.Add(maxInput.Element);

            minInput.OnChanged(newValue =>
            {
                float snapped = Mathf.Min(Snap(newValue), maxInput.Value);
                if (!Mathf.Approximately(snapped, newValue)) minInput.Value = snapped;
                else slider.SetValueWithoutNotify(new Vector2(minInput.Value, maxInput.Value));
            });

            maxInput.OnChanged(newValue =>
            {
                float snapped = Mathf.Max(Snap(newValue), minInput.Value);
                if (!Mathf.Approximately(snapped, newValue)) maxInput.Value = snapped;
                else slider.SetValueWithoutNotify(new Vector2(minInput.Value, maxInput.Value));
            });

            slider.RegisterValueChangedCallback(changeEvent =>
            {
                float snappedMin = Snap(changeEvent.newValue.x);
                float snappedMax = Snap(changeEvent.newValue.y);

                // 数値欄への代入が BindProperty 経由でプロパティに書き戻る
                minInput.Value = snappedMin;
                maxInput.Value = snappedMax;
                slider.SetValueWithoutNotify(new Vector2(snappedMin, snappedMax));
            });

            // Undo などで外から値が変わったときもスライダーを追従させる
            slider.TrackPropertyValue(context.Property, tracked =>
            {
                SerializedProperty min = tracked.serializedObject.FindProperty(minPath);
                SerializedProperty max = tracked.serializedObject.FindProperty(maxPath);
                if (min == null || max == null) return;

                slider.SetValueWithoutNotify(new Vector2(ReadValue(min), ReadValue(max)));
            });

            return FieldFactoryResult.Accept(slider, slider.labelElement);
        }

        private static SerializedProperty FindChild(SerializedProperty property, string primaryName, string fallbackName)
            => property.FindPropertyRelative(primaryName) ?? property.FindPropertyRelative(fallbackName);

        private static float ReadValue(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.Integer ? property.intValue : property.floatValue;

        private static ValueInput CreateInput(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                IntegerField integerField = new IntegerField { isDelayed = true };
                integerField.BindProperty(property);
                return new ValueInput(
                    integerField,
                    () => integerField.value,
                    value => integerField.value = Mathf.RoundToInt(value),
                    callback => integerField.RegisterValueChangedCallback(changeEvent => callback(changeEvent.newValue)));
            }

            FloatField floatField = new FloatField { isDelayed = true };
            floatField.BindProperty(property);
            return new ValueInput(
                floatField,
                () => floatField.value,
                value => floatField.value = value,
                callback => floatField.RegisterValueChangedCallback(changeEvent => callback(changeEvent.newValue)));
        }

        /// <summary>float と int の数値欄を、float で読み書きできるようにそろえる。</summary>
        private sealed class ValueInput
        {
            private readonly Func<float> _getter;
            private readonly Action<float> _setter;
            private readonly Action<Action<float>> _subscribe;

            public VisualElement Element { get; }

            public float Value
            {
                get => _getter();
                set => _setter(value);
            }

            public ValueInput(
                VisualElement element, Func<float> getter, Action<float> setter, Action<Action<float>> subscribe)
            {
                Element = element;
                Element.AddToClassList(InspectorClassNames.MIN_MAX_VALUE);
                _getter = getter;
                _setter = setter;
                _subscribe = subscribe;
            }

            public void OnChanged(Action<float> callback) => _subscribe(callback);
        }
    }
}
