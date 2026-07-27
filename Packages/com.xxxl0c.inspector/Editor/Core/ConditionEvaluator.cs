using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 兄弟フィールドの値と期待値を比較する。[ShowIf] / [HideIf] が共通で使う。
    /// </summary>
    public static class ConditionEvaluator
    {
        private const double EPSILON = 1e-6;

        /// <summary>
        /// 条件を評価する。評価できた場合だけ true を返し、結果を <paramref name="matched"/> に入れる。
        /// 評価できなかった場合は false を返し、理由を <paramref name="reason"/> に入れる。
        /// </summary>
        public static bool TryEvaluate(
            SerializedProperty property,
            string fieldName,
            object expectedValue,
            out bool matched,
            out string reason)
        {
            matched = false;
            reason = null;

            if (string.IsNullOrEmpty(fieldName))
            {
                reason = "条件フィールドが指定されていません。";
                return false;
            }

            // 兄弟は必ず親パスからの相対で引く。ルートから探すと配列要素内・ネストクラス内で壊れる
            SerializedProperty condition = PropertyPathUtility.FindSibling(property, fieldName);
            if (condition == null)
            {
                reason = $"条件フィールド '{fieldName}' が見つかりません。nameof で指定しているか確認してください。";
                return false;
            }

            if (expectedValue == null)
            {
                if (condition.propertyType != SerializedPropertyType.Boolean)
                {
                    reason = $"条件フィールド '{fieldName}' は bool ではないため、比較する値の指定が必要です。";
                    return false;
                }

                matched = condition.boolValue;
                return true;
            }

            switch (condition.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    if (expectedValue is bool expectedBool)
                    {
                        matched = condition.boolValue == expectedBool;
                        return true;
                    }
                    break;

                case SerializedPropertyType.Enum:
                    if (TryToLong(expectedValue, out long expectedEnum))
                    {
                        matched = condition.intValue == expectedEnum;
                        return true;
                    }
                    break;

                case SerializedPropertyType.Integer:
                    if (TryToLong(expectedValue, out long expectedInteger))
                    {
                        matched = condition.longValue == expectedInteger;
                        return true;
                    }
                    break;

                case SerializedPropertyType.Float:
                    if (TryToDouble(expectedValue, out double expectedFloat))
                    {
                        matched = Math.Abs(condition.doubleValue - expectedFloat) < EPSILON;
                        return true;
                    }
                    break;

                case SerializedPropertyType.String:
                    if (expectedValue is string expectedString)
                    {
                        matched = string.Equals(condition.stringValue, expectedString, StringComparison.Ordinal);
                        return true;
                    }
                    break;

                case SerializedPropertyType.ObjectReference:
                    // 「参照が入っているか」を条件にできるようにする
                    if (expectedValue is bool expectedAssigned)
                    {
                        matched = (condition.objectReferenceValue != null) == expectedAssigned;
                        return true;
                    }
                    break;
            }

            reason = $"条件フィールド '{fieldName}'（{condition.propertyType}）と指定した値の型が合いません。";
            return false;
        }

        private static bool TryToLong(object value, out long result)
        {
            result = 0;
            if (value == null) return false;

            if (value.GetType().IsEnum
                || value is sbyte || value is byte
                || value is short || value is ushort
                || value is int || value is uint
                || value is long)
            {
                result = Convert.ToInt64(value);
                return true;
            }

            return false;
        }

        private static bool TryToDouble(object value, out double result)
        {
            result = 0;
            if (value is float floatValue)
            {
                result = floatValue;
                return true;
            }

            if (value is double doubleValue)
            {
                result = doubleValue;
                return true;
            }

            if (TryToLong(value, out long longValue))
            {
                result = longValue;
                return true;
            }

            return false;
        }
    }
}
