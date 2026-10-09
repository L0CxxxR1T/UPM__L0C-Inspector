using System;
using UnityEditor;
using UnityEditor.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[MinValue] / [MaxValue] の検証と丸めの共通部分。</summary>
    internal static class NumericBoundsValidation
    {
        public static void Validate(ValidationContext context, double bound, bool isMin, string attributeName)
        {
            SerializedProperty property = context.Property;

            switch (context.Role)
            {
                case CollectionRole.Collection:
                case CollectionRole.DictionaryKey:
                    return;

                case CollectionRole.Dictionary:
                    DictionaryUtility.TryGetKeyValueTypes(context.ValueType, out Type _, out Type valueType);
                    if (!NumericBoundsUtility.IsSupported(valueType))
                    {
                        context.Report(
                            ValidationSeverity.Warning,
                            $"[{attributeName}] は値が数値でない Dictionary には効果がありません（キーには効きません）。");
                    }
                    return;

                case CollectionRole.DictionaryValue:
                    if (!NumericBoundsUtility.IsSupported(property.propertyType)) return;
                    break;

                default:
                    if (!NumericBoundsUtility.IsSupported(property.propertyType))
                    {
                        context.Report(
                            ValidationSeverity.Warning,
                            $"[{attributeName}] は {property.propertyType} 型には効果がありません。属性を外してください。");
                        return;
                    }
                    break;
            }

            if (!NumericBoundsUtility.TryFindViolation(property, bound, isMin, out string actual)) return;

            string limit = NumericBoundsUtility.FormatBound(bound);
            string condition = isMin ? $"{limit} 以上" : $"{limit} 以下";
            context.Report(ValidationSeverity.Error, $"{context.Label} は {condition}にしてください（現在 {actual}）。");
        }

        /// <summary>
        /// インスペクタで範囲外の値が入ったら境界に揃える。配列・List は要素ごと、Dictionary は値ごとに見る。
        /// </summary>
        public static void TrackAndClamp(DecorationContext context, double bound, bool isMin)
        {
            bool isDictionary = DictionaryUtility.IsSerializedDictionary(context.FieldInfo.FieldType);

            context.Row.TrackPropertyValue(context.Property, changed =>
            {
                // 複数選択中に値が食い違っていると、どの値を基準に揃えるか決められない
                if (changed.hasMultipleDifferentValues) return;

                if (ClampTree(changed, bound, isMin, isDictionary)) changed.serializedObject.ApplyModifiedProperties();
            });
        }

        private static bool ClampTree(SerializedProperty property, double bound, bool isMin, bool isDictionary)
        {
            if (!property.isArray || property.propertyType == SerializedPropertyType.String)
            {
                return NumericBoundsUtility.Clamp(property, bound, isMin);
            }

            bool changed = false;
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                if (isDictionary) element = element.FindPropertyRelative(DictionaryUtility.VALUE_NAME);

                changed |= NumericBoundsUtility.Clamp(element, bound, isMin);
            }

            return changed;
        }
    }
}
