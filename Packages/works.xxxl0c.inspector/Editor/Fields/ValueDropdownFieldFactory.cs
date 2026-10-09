using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [ValueDropdown] のフィールドファクトリ。static readonly フィールドに並べた候補をドロップダウンで選ばせる。
    /// 候補に無い値が入っている場合は、その値を「候補外」と添えて表示する（検証層も Warning を出す）。
    /// </summary>
    public sealed class ValueDropdownFieldFactory : IFieldFactory
    {
        private const string OUT_OF_CHOICES_SUFFIX = "（候補外）";

        public Type AttributeType => typeof(ValueDropdownAttribute);

        public FieldFactoryResult Create(FieldFactoryContext context)
        {
            SerializedProperty property = context.Property;
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                return FieldFactoryResult.Decline("[ValueDropdown] は配列・List には未対応です。");
            }

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                return FieldFactoryResult.Decline("[ValueDropdown] は [SerializeReference] には使えません。");
            }

            ValueDropdownAttribute attribute = (ValueDropdownAttribute)context.Attribute;
            if (!ValueDropdownSource.TryGetEntries(
                    context.FieldInfo.DeclaringType, context.FieldInfo.FieldType, attribute,
                    out List<ValueDropdownEntry> entries, out string reason))
            {
                return FieldFactoryResult.Decline($"[ValueDropdown] {reason}");
            }

            List<string> choices = BuildUniqueTexts(entries);
            DropdownField dropdown = new DropdownField(property.displayName, choices, -1);
            dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            dropdown.AddToClassList(InspectorClassNames.VALUE_DROPDOWN);

            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;

            void Sync(SerializedProperty current)
            {
                dropdown.showMixedValue = current.hasMultipleDifferentValues;

                int index = ValueDropdownSource.IndexOf(entries, current);
                dropdown.SetValueWithoutNotify(index >= 0
                    ? choices[index]
                    : ValueDropdownSource.FormatCurrent(current) + OUT_OF_CHOICES_SUFFIX);
            }

            dropdown.RegisterValueChangedCallback(changeEvent =>
            {
                int index = choices.IndexOf(changeEvent.newValue);
                if (index < 0) return;

                SerializedProperty current = serializedObject.FindProperty(propertyPath);
                if (current == null) return;

                ValueDropdownSource.Assign(current, entries[index].Value);
                serializedObject.ApplyModifiedProperties();
            });

            dropdown.TrackPropertyValue(property, Sync);
            Sync(property);

            return FieldFactoryResult.Accept(dropdown, dropdown.labelElement);
        }

        /// <summary>ドロップダウンは文字列で選択を区別するので、同じ表示名には番号を添えて区別する。</summary>
        private static List<string> BuildUniqueTexts(List<ValueDropdownEntry> entries)
        {
            List<string> texts = new List<string>(entries.Count);
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (ValueDropdownEntry entry in entries)
            {
                counts.TryGetValue(entry.Text, out int count);
                counts[entry.Text] = count + 1;
                texts.Add(count == 0 ? entry.Text : $"{entry.Text} ({count + 1})");
            }

            return texts;
        }
    }
}
