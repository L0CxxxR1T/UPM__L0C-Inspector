using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [MinValue] / [MaxValue] が共有する、数値の範囲判定と丸め。Vector 系は成分ごとに扱う。
    /// </summary>
    /// <remarks>
    /// float のフィールドは float に丸めてから比べる。double のまま比べると、0.1 のような境界値で
    /// 「保存できる一番近い値」が永遠に範囲外と判定されてしまう。
    /// </remarks>
    internal static class NumericBoundsUtility
    {
        private const string DOUBLE_TYPE_NAME = "double";

        public static bool IsSupported(SerializedPropertyType type)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>C# の型として範囲判定に対応しているか。Dictionary の値の型を調べるのに使う。</summary>
        public static bool IsSupported(Type type)
        {
            if (type == null || type.IsEnum) return false;
            if (type.IsPrimitive) return type != typeof(bool) && type != typeof(char);

            return type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4)
                || type == typeof(Vector2Int) || type == typeof(Vector3Int);
        }

        /// <summary>範囲外の成分があれば、その値を表示用の文字列で返す。</summary>
        public static bool TryFindViolation(SerializedProperty property, double bound, bool isMin, out string actual)
        {
            foreach (SerializedProperty scalar in EnumerateScalars(property))
            {
                if (!IsViolated(scalar, bound, isMin)) continue;

                actual = Format(scalar);
                return true;
            }

            actual = null;
            return false;
        }

        /// <summary>範囲外の成分を境界の値に揃える。変更があれば true（ApplyModifiedProperties は呼び出し側）。</summary>
        public static bool Clamp(SerializedProperty property, double bound, bool isMin)
        {
            bool changed = false;
            foreach (SerializedProperty scalar in EnumerateScalars(property))
            {
                if (!IsViolated(scalar, bound, isMin)) continue;

                SetToBound(scalar, bound, isMin);
                changed = true;
            }

            return changed;
        }

        public static string FormatBound(double bound) => bound.ToString(CultureInfo.InvariantCulture);

        private static IEnumerable<SerializedProperty> EnumerateScalars(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Integer
                || property.propertyType == SerializedPropertyType.Float)
            {
                yield return property;
                yield break;
            }

            if (!IsSupported(property.propertyType)) yield break;

            // Vector 系の子（x / y / z / w）は Integer か Float
            foreach (SerializedProperty child in SerializedFieldUtility.EnumerateDirectChildren(property))
            {
                yield return child;
            }
        }

        private static bool IsViolated(SerializedProperty scalar, double bound, bool isMin)
        {
            if (scalar.propertyType == SerializedPropertyType.Integer)
            {
                long value = scalar.longValue;
                long limit = ToIntegerBound(bound, isMin);
                return isMin ? value < limit : value > limit;
            }

            if (IsDouble(scalar))
            {
                double value = scalar.doubleValue;
                return isMin ? value < bound : value > bound;
            }

            float single = scalar.floatValue;
            float singleBound = (float)bound;
            return isMin ? single < singleBound : single > singleBound;
        }

        private static void SetToBound(SerializedProperty scalar, double bound, bool isMin)
        {
            if (scalar.propertyType == SerializedPropertyType.Integer)
            {
                scalar.longValue = ToIntegerBound(bound, isMin);
            }
            else if (IsDouble(scalar))
            {
                scalar.doubleValue = bound;
            }
            else
            {
                scalar.floatValue = (float)bound;
            }
        }

        /// <summary>整数の範囲に入る一番端の値。下限なら切り上げ、上限なら切り捨てる。</summary>
        private static long ToIntegerBound(double bound, bool isMin)
        {
            double rounded = isMin ? Math.Ceiling(bound) : Math.Floor(bound);
            if (rounded <= long.MinValue) return long.MinValue;
            if (rounded >= long.MaxValue) return long.MaxValue;

            return (long)rounded;
        }

        private static bool IsDouble(SerializedProperty scalar)
            => string.Equals(scalar.type, DOUBLE_TYPE_NAME, StringComparison.Ordinal);

        private static string Format(SerializedProperty scalar)
        {
            if (scalar.propertyType == SerializedPropertyType.Integer)
            {
                return scalar.longValue.ToString(CultureInfo.InvariantCulture);
            }

            return IsDouble(scalar)
                ? scalar.doubleValue.ToString(CultureInfo.InvariantCulture)
                : scalar.floatValue.ToString(CultureInfo.InvariantCulture);
        }
    }
}
