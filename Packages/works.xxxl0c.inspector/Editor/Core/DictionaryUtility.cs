using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// Unity 6.6 の標準 Dictionary シリアライズを扱う補助。
    /// SerializedProperty 上では「key / value を持つ要素の配列」として現れる。
    /// </summary>
    /// <remarks>
    /// キーの比較は SerializedProperty の値を正規化して行う。enum キーは boxedValue が int を返すなど、
    /// C# の型と SerializedProperty の表現が一致しないため、両者を同じ表現に揃えてから比べる。
    /// </remarks>
    public static class DictionaryUtility
    {
        public const string KEY_NAME = "key";
        public const string VALUE_NAME = "value";

        private const string ULONG_TYPE_NAME = "ulong";

        /// <summary>
        /// Unity がシリアライズする Dictionary 型か。6.6 未満は Dictionary をシリアライズしないので常に false。
        /// 宣言型が Dictionary そのものでなければならない（派生クラスは対象外）のは Unity の仕様に合わせている。
        /// </summary>
        public static bool IsSerializedDictionary(Type type)
        {
#if UNITY_6000_6_OR_NEWER
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
#else
            return false;
#endif
        }

        public static bool TryGetKeyValueTypes(Type dictionaryType, out Type keyType, out Type valueType)
        {
            if (!IsSerializedDictionary(dictionaryType))
            {
                keyType = null;
                valueType = null;
                return false;
            }

            Type[] arguments = dictionaryType.GetGenericArguments();
            keyType = arguments[0];
            valueType = arguments[1];
            return true;
        }

        /// <summary>
        /// キーが null か。Object 参照のキーだけが null になり得る（参照切れも含む）。
        /// null キーの要素は実行時の Dictionary に入らない。
        /// </summary>
        public static bool IsNullKey(SerializedProperty key)
            => key.propertyType == SerializedPropertyType.ObjectReference && key.objectReferenceValue == null;

        /// <summary>キーを比較用の表現で読む。null キーなら null。</summary>
        public static object ReadComparableKey(SerializedProperty key)
        {
            switch (key.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return string.Equals(key.type, ULONG_TYPE_NAME, StringComparison.Ordinal)
                        ? (object)key.ulongValue
                        : key.longValue;
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Character:
                    return (long)key.intValue;
                case SerializedPropertyType.Boolean:
                    return key.boolValue;
                case SerializedPropertyType.Float:
                    return key.doubleValue;
                case SerializedPropertyType.String:
                    return key.stringValue;
                case SerializedPropertyType.ObjectReference:
                    return key.objectReferenceValue;
                default:
                    return key.boxedValue;
            }
        }

        /// <summary>C# の値を <see cref="ReadComparableKey"/> と同じ表現に揃える。</summary>
        public static object ToComparableKey(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case Enum _:
                case char _:
                    return Convert.ToInt64(value);
                case ulong unsignedLong:
                    return unsignedLong;
                case sbyte _:
                case byte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                    return Convert.ToInt64(value);
                case float _:
                case double _:
                    return Convert.ToDouble(value);
                default:
                    return value;
            }
        }

        /// <summary>同じキーを持つ要素の添字を返す。null キーの要素は比較対象外。無ければ -1。</summary>
        public static int IndexOfKey(SerializedProperty dictionary, object comparableKey)
        {
            for (int i = 0; i < dictionary.arraySize; i++)
            {
                SerializedProperty key = dictionary.GetArrayElementAtIndex(i).FindPropertyRelative(KEY_NAME);
                if (IsNullKey(key)) continue;

                if (Equals(ReadComparableKey(key), comparableKey)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 重複しているキーごとに、該当する要素の添字をまとめて返す。
        /// キー型の Equals / GetHashCode がユーザー実装の場合、そこで投げた例外はそのまま伝わる。
        /// </summary>
        public static List<List<int>> FindDuplicateGroups(SerializedProperty dictionary)
        {
            Dictionary<object, List<int>> groups = new Dictionary<object, List<int>>();
            for (int i = 0; i < dictionary.arraySize; i++)
            {
                SerializedProperty key = dictionary.GetArrayElementAtIndex(i).FindPropertyRelative(KEY_NAME);
                if (IsNullKey(key)) continue;

                object comparable = ReadComparableKey(key);
                if (!groups.TryGetValue(comparable, out List<int> indices))
                {
                    indices = new List<int>();
                    groups.Add(comparable, indices);
                }

                indices.Add(i);
            }

            List<List<int>> duplicates = new List<List<int>>();
            foreach (List<int> indices in groups.Values)
            {
                if (indices.Count > 1) duplicates.Add(indices);
            }

            return duplicates;
        }

        /// <summary>キーに値を書き込む。value は C# 側のキー型の値。</summary>
        public static void WriteKey(SerializedProperty key, object value)
        {
            switch (key.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (value is ulong unsignedLong) key.ulongValue = unsignedLong;
                    else key.longValue = Convert.ToInt64(value);
                    break;
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Character:
                    key.intValue = Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Boolean:
                    key.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Float:
                    key.doubleValue = Convert.ToDouble(value);
                    break;
                case SerializedPropertyType.String:
                    key.stringValue = (string)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    key.objectReferenceValue = (Object)value;
                    break;
                default:
                    key.boxedValue = value;
                    break;
            }
        }

        /// <summary>
        /// 値を既定値に戻す。配列の末尾に要素を足すと直前の要素の中身が複製されるので、追加直後に呼ぶ。
        /// 既定値を作れない型（引数なしコンストラクタが無いクラスなど）は例外を投げる。
        /// </summary>
        public static void ResetValue(SerializedProperty value, Type valueType)
        {
            switch (value.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    value.objectReferenceValue = null;
                    return;
                case SerializedPropertyType.String:
                    value.stringValue = string.Empty;
                    return;
                case SerializedPropertyType.Integer:
                    value.longValue = 0;
                    return;
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Character:
                    value.intValue = 0;
                    return;
                case SerializedPropertyType.Boolean:
                    value.boolValue = false;
                    return;
                case SerializedPropertyType.Float:
                    value.doubleValue = 0;
                    return;
            }

            // List / 配列 / 入れ子の Dictionary
            if (value.isArray)
            {
                value.ClearArray();
                return;
            }

            value.boxedValue = valueType.IsValueType
                ? Activator.CreateInstance(valueType)
                : Activator.CreateInstance(valueType, true);
        }

        /// <summary>メッセージ内で要素を指すためのキーの表記。表せないキーは添字で示す。</summary>
        public static string FormatKey(SerializedProperty key, int index)
        {
            switch (key.propertyType)
            {
                case SerializedPropertyType.String:
                    return $"\"{key.stringValue}\"";
                case SerializedPropertyType.Integer:
                    return string.Equals(key.type, ULONG_TYPE_NAME, StringComparison.Ordinal)
                        ? key.ulongValue.ToString()
                        : key.longValue.ToString();
                case SerializedPropertyType.Float:
                    return key.doubleValue.ToString();
                case SerializedPropertyType.Boolean:
                    return key.boolValue.ToString();
                case SerializedPropertyType.Enum:
                    return key.enumValueIndex >= 0 && key.enumValueIndex < key.enumDisplayNames.Length
                        ? key.enumDisplayNames[key.enumValueIndex]
                        : key.intValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return key.objectReferenceValue != null ? key.objectReferenceValue.name : $"#{index}";
                default:
                    return $"#{index}";
            }
        }

        /// <summary>
        /// [Required] が意味を持つ Dictionary か。キーか値（コレクションならその要素）が null になり得る型なら true。
        /// </summary>
        public static bool CanContainNull(Type dictionaryType)
        {
            if (!TryGetKeyValueTypes(dictionaryType, out Type keyType, out Type valueType)) return false;

            return IsNullableReference(keyType) || IsNullableReference(valueType);
        }

        private static bool IsNullableReference(Type type)
        {
            if (typeof(Object).IsAssignableFrom(type)) return true;
            if (IsSerializedDictionary(type)) return CanContainNull(type);

            Type elementType = SerializedFieldUtility.GetCollectionElementType(type);
            return elementType != null && typeof(Object).IsAssignableFrom(elementType);
        }
    }
}
