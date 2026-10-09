using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[EnumToggleButtons] のフィールドファクトリ。enum を横並びのボタン（ToggleButtonGroup）で表示する。</summary>
    public sealed class EnumToggleButtonsFieldFactory : IFieldFactory
    {
        // ToggleButtonGroupState が扱える選択肢の上限
        private const int MAX_OPTIONS = 64;

        public Type AttributeType => typeof(EnumToggleButtonsAttribute);

        public FieldFactoryResult Create(FieldFactoryContext context)
        {
            SerializedProperty property = context.Property;
            Type enumType = context.FieldInfo.FieldType;

            if (property.propertyType != SerializedPropertyType.Enum || !enumType.IsEnum)
            {
                return FieldFactoryResult.Decline(
                    $"[EnumToggleButtons] は enum 型にのみ使えます（{property.propertyType} には使えません）。");
            }

            Type underlying = Enum.GetUnderlyingType(enumType);
            if (underlying == typeof(long) || underlying == typeof(ulong))
            {
                return FieldFactoryResult.Decline("[EnumToggleButtons] は long / ulong を基にした enum には未対応です。");
            }

            bool isFlags = EnumToggleOptions.IsFlags(enumType);
            List<EnumToggleOption> options = EnumToggleOptions.Build(enumType);
            if (options.Count == 0 || options.Count > MAX_OPTIONS)
            {
                return FieldFactoryResult.Decline(
                    $"[EnumToggleButtons] でボタンにできる値は 1〜{MAX_OPTIONS} 個です（{options.Count} 個あります）。");
            }

            ToggleButtonGroup group = new ToggleButtonGroup(property.displayName)
            {
                isMultipleSelection = isFlags,
                allowEmptySelection = isFlags
            };
            group.AddToClassList(BaseField<ToggleButtonGroupState>.alignedFieldUssClassName);
            group.AddToClassList(InspectorClassNames.ENUM_TOGGLE_BUTTONS);

            foreach (EnumToggleOption option in options)
            {
                group.Add(new Button { text = option.DisplayName });
            }

            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;

            void Sync(SerializedProperty current)
            {
                group.showMixedValue = current.hasMultipleDifferentValues;
                List<bool> selection = EnumToggleOptions.ToSelection(options, current.intValue, isFlags);
                group.SetValueWithoutNotify(ToggleButtonGroupState.CreateFromOptions(selection));
            }

            group.RegisterValueChangedCallback(changeEvent =>
            {
                SerializedProperty current = serializedObject.FindProperty(propertyPath);
                if (current == null) return;

                List<bool> selection = new List<bool>(options.Count);
                for (int i = 0; i < options.Count; i++) selection.Add(changeEvent.newValue[i]);

                current.intValue = (int)EnumToggleOptions.FromSelection(options, selection, current.intValue, isFlags);
                serializedObject.ApplyModifiedProperties();
            });

            group.TrackPropertyValue(property, Sync);
            Sync(property);

            return FieldFactoryResult.Accept(group, group.labelElement);
        }
    }
}
