using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[ValueDropdown] の候補1件。</summary>
    internal readonly struct ValueDropdownEntry
    {
        public string Text { get; }
        public object Value { get; }

        public ValueDropdownEntry(string text, object value)
        {
            Text = text;
            Value = value;
        }
    }

    /// <summary>
    /// [ValueDropdown] の候補の読み出しと、SerializedProperty の値との比較・代入。
    /// 候補は static readonly フィールドからしか読まない（検証のときにユーザーコードを実行しないため）。
    /// </summary>
    internal static class ValueDropdownSource
    {
        private const BindingFlags SOURCE_FLAGS =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        private const string DOUBLE_TYPE_NAME = "double";

        /// <param name="ownerType">属性の付いたフィールドを宣言している型。SourceType 未指定のときの取得元。</param>
        /// <param name="valueType">フィールドの値の型。候補の値がこの型に入るかを確かめる。</param>
        public static bool TryGetEntries(
            Type ownerType,
            Type valueType,
            ValueDropdownAttribute attribute,
            out List<ValueDropdownEntry> entries,
            out string reason)
        {
            entries = null;
            Type sourceType = attribute.SourceType ?? ownerType;

            if (string.IsNullOrEmpty(attribute.FieldName))
            {
                reason = "候補のフィールドが指定されていません。";
                return false;
            }

            FieldInfo source = sourceType.GetField(attribute.FieldName, SOURCE_FLAGS);
            if (source == null)
            {
                reason = $"候補のフィールド '{attribute.FieldName}' が {sourceType.Name} に見つかりません。"
                    + "nameof で指定しているか確認してください。";
                return false;
            }

            if (!source.IsStatic || !source.IsInitOnly)
            {
                reason = $"候補のフィールド '{attribute.FieldName}' は static readonly にしてください。";
                return false;
            }

            object raw;
            try
            {
                raw = source.GetValue(null);
            }
            catch (TargetInvocationException exception)
            {
                // 静的コンストラクタで例外が出た。インスペクタ全体は落とさず理由として返す
                Exception inner = exception.InnerException ?? exception;
                reason = $"候補のフィールド '{attribute.FieldName}' を読めませんでした: {inner.Message}";
                return false;
            }

            if (!(raw is IEnumerable enumerable) || raw is string)
            {
                reason = $"候補のフィールド '{attribute.FieldName}' は配列か List にしてください。";
                return false;
            }

            entries = new List<ValueDropdownEntry>();
            foreach (object item in enumerable)
            {
                ValueDropdownEntry entry = item is IValueDropdownItem named
                    ? new ValueDropdownEntry(named.Text, named.Value)
                    : new ValueDropdownEntry(FormatValue(item), item);

                if (!IsAssignable(entry.Value, valueType))
                {
                    string actual = entry.Value == null ? "null" : entry.Value.GetType().Name;
                    reason = $"候補 '{entry.Text}' の型 {actual} はフィールドの型 {valueType.Name} に入りません。";
                    entries = null;
                    return false;
                }

                entries.Add(entry);
            }

            if (entries.Count == 0)
            {
                reason = $"候補のフィールド '{attribute.FieldName}' が空です。";
                entries = null;
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>現在の値と一致する候補の位置。無ければ -1。</summary>
        public static int IndexOf(List<ValueDropdownEntry> entries, SerializedProperty property)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (Matches(property, entries[i].Value)) return i;
            }

            return -1;
        }

        public static void Assign(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.longValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                    break;
                case SerializedPropertyType.Enum:
                    property.intValue = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Float:
                    property.doubleValue = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value ?? string.Empty;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as Object;
                    break;
                default:
                    property.boxedValue = value;
                    break;
            }
        }

        /// <summary>メッセージや候補外の表示に使う、現在の値の文字列。</summary>
        public static string FormatCurrent(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.longValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Enum:
                    int index = property.enumValueIndex;
                    return index >= 0 && index < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[index]
                        : property.intValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Float:
                    return IsDouble(property)
                        ? property.doubleValue.ToString(CultureInfo.InvariantCulture)
                        : property.floatValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.String:
                    return $"\"{property.stringValue}\"";
                case SerializedPropertyType.ObjectReference:
                    return FormatValue(property.objectReferenceValue);
                default:
                    return FormatValue(property.boxedValue);
            }
        }

        private static bool Matches(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return IsIntegral(value)
                        && Convert.ToInt64(value, CultureInfo.InvariantCulture) == property.longValue;
                case SerializedPropertyType.Enum:
                    return value is Enum && Convert.ToInt64(value, CultureInfo.InvariantCulture) == property.intValue;
                case SerializedPropertyType.Boolean:
                    return value is bool flag && flag == property.boolValue;
                case SerializedPropertyType.Float:
                    if (!(value is float) && !(value is double)) return false;

                    // float のフィールドは float に丸めた候補と比べる（double のままだと 0.1 などが一致しない）
                    double candidate = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    return IsDouble(property)
                        ? candidate.Equals(property.doubleValue)
                        : ((float)candidate).Equals(property.floatValue);
                case SerializedPropertyType.String:
                    return string.Equals(property.stringValue, (string)value ?? string.Empty, StringComparison.Ordinal);
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == value as Object;
                default:
                    return Equals(property.boxedValue, value);
            }
        }

        private static bool IsAssignable(object value, Type valueType)
        {
            if (value == null) return !valueType.IsValueType;

            return valueType.IsInstanceOfType(value);
        }

        private static bool IsIntegral(object value)
            => value is sbyte || value is byte || value is short || value is ushort
                || value is int || value is uint || value is long || value is ulong;

        private static bool IsDouble(SerializedProperty property)
            => string.Equals(property.type, DOUBLE_TYPE_NAME, StringComparison.Ordinal);

        private static string FormatValue(object value)
        {
            switch (value)
            {
                case null:
                    return "null";
                case Object unityObject:
                    return unityObject == null ? "null" : unityObject.name;
                case IFormattable formattable:
                    return formattable.ToString(null, CultureInfo.InvariantCulture);
                default:
                    return value.ToString();
            }
        }
    }
}
