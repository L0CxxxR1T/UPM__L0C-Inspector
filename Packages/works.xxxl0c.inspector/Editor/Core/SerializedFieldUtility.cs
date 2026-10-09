using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// SerializedProperty と C# のフィールド／型を突き合わせるためのリフレクション補助。
    /// </summary>
    public static class SerializedFieldUtility
    {
        private const BindingFlags FIELD_FLAGS =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 型とその基底クラスのインスタンスフィールドを列挙する。
        /// UnityEngine 側の基底クラスまで降りても意味が無いので、そこで打ち切る。
        /// </summary>
        public static IEnumerable<FieldInfo> EnumerateSerializedFields(Type type)
        {
            Type current = type;
            while (current != null && current != typeof(object) && !IsFrameworkNamespace(current.Namespace))
            {
                foreach (FieldInfo field in current.GetFields(FIELD_FLAGS))
                {
                    if (field.IsNotSerialized) continue;
                    yield return field;
                }

                current = current.BaseType;
            }
        }

        /// <summary>名前でフィールドを引く。基底クラスも辿る。見つからなければ null。</summary>
        public static FieldInfo FindSerializedField(Type ownerType, string fieldName)
        {
            if (ownerType == null || string.IsNullOrEmpty(fieldName)) return null;

            foreach (FieldInfo field in EnumerateSerializedFields(ownerType))
            {
                if (string.Equals(field.Name, fieldName, StringComparison.Ordinal)) return field;
            }

            return null;
        }

        /// <summary>
        /// 直接の子プロパティだけを列挙する。
        /// NextVisible ではなく Next を使うのは、NextVisible が Foldout の開閉状態に依存するため。
        /// 折りたたんだだけで構造の走査結果が変わってしまうのを避ける。
        /// </summary>
        public static IEnumerable<SerializedProperty> EnumerateDirectChildren(SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            int childDepth = property.depth + 1;
            bool enterChildren = true;

            while (iterator.Next(enterChildren))
            {
                enterChildren = false;
                if (SerializedProperty.EqualContents(iterator, end)) break;
                if (iterator.depth < childDepth) break;

                yield return iterator.Copy();
            }
        }

        /// <summary>配列 / List&lt;T&gt; の要素型を返す。コレクションでなければ null。</summary>
        public static Type GetCollectionElementType(Type type)
        {
            if (type == null) return null;
            if (type.IsArray) return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        /// <summary>
        /// [SerializeReference] の実際に入っている型を返す。未設定なら null。
        /// managedReferenceFullTypename は "アセンブリ名 型のフルネーム" 形式。
        /// </summary>
        public static Type ResolveManagedReferenceType(SerializedProperty property)
        {
            string fullTypename = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypename)) return null;

            int separator = fullTypename.IndexOf(' ');
            if (separator < 0) return null;

            string assemblyName = fullTypename.Substring(0, separator);
            string typeName = fullTypename.Substring(separator + 1);
            return Type.GetType($"{typeName}, {assemblyName}");
        }

        /// <summary>
        /// その型がインラインでシリアライズされる自前のフィールドを持ち得るか。
        /// Object 参照は別アセットなので降りない。プリミティブ・フレームワーク型も対象外。
        /// </summary>
        public static bool CanContainSerializedFields(Type type)
        {
            if (type == null) return false;
            if (type.IsPrimitive || type.IsEnum || type == typeof(string)) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;

            return !IsFrameworkNamespace(type.Namespace);
        }

        /// <summary>
        /// property の値そのものではなく、その値を宣言している C# インスタンスを辿って返す。
        /// [OnValueChanged] のようにフィールドの持ち主でメソッドを呼びたいときに使う。
        /// 途中で null 参照や解決不能な段があれば null。
        /// </summary>
        public static object ResolveOwnerObject(SerializedProperty property)
        {
            List<PathStep> steps = ParsePathSteps(property.propertyPath);
            if (steps.Count == 0) return null;

            object current = property.serializedObject.targetObject;

            // 最後の1段は「持ち主から見た自分自身」を表すので、そこへ降りる前で止める
            for (int i = 0; i < steps.Count - 1; i++)
            {
                PathStep step = steps[i];

                if (step.IsIndex && current is IDictionary dictionary)
                {
                    // Dictionary の要素は ".Array.data[i].key/value" の2段で表れる。
                    // 自分自身がキーか値なら、持ち主は C# 上に存在しない要素なので辿れない
                    if (i + 1 >= steps.Count - 1) return null;

                    current = ApplyDictionaryStep(dictionary, property.serializedObject, step, steps[i + 1]);
                    i++;
                }
                else
                {
                    current = ApplyStep(current, step);
                }

                if (current == null) return null;
            }

            return current;
        }

        /// <summary>
        /// propertyPath を「フィールド名」または「配列添字」の列に分解する。
        /// ".Array.data[3]" は1つの添字ステップにまとめる（Unity のパス表現をそのまま辿ると壊れるため）。
        /// </summary>
        private static List<PathStep> ParsePathSteps(string propertyPath)
        {
            const string ARRAY_TOKEN = "Array";
            const string DATA_PREFIX = "data[";

            List<PathStep> steps = new List<PathStep>();
            string[] tokens = propertyPath.Split('.');
            string path = string.Empty;

            int i = 0;
            while (i < tokens.Length)
            {
                if (string.Equals(tokens[i], ARRAY_TOKEN, StringComparison.Ordinal)
                    && i + 1 < tokens.Length
                    && tokens[i + 1].StartsWith(DATA_PREFIX, StringComparison.Ordinal))
                {
                    string token = tokens[i + 1];
                    int index = int.Parse(token.Substring(DATA_PREFIX.Length, token.Length - DATA_PREFIX.Length - 1));
                    path = $"{path}.{ARRAY_TOKEN}.{token}";
                    steps.Add(new PathStep(null, index, path));
                    i += 2;
                    continue;
                }

                path = path.Length == 0 ? tokens[i] : $"{path}.{tokens[i]}";
                steps.Add(new PathStep(tokens[i], -1, path));
                i++;
            }

            return steps;
        }

        private static object ApplyStep(object current, PathStep step)
        {
            if (current == null) return null;

            if (step.IsIndex)
            {
                if (!(current is IList list) || step.Index >= list.Count) return null;
                return list[step.Index];
            }

            FieldInfo field = FindSerializedField(current.GetType(), step.Name);
            return field?.GetValue(current);
        }

        /// <summary>
        /// Dictionary の要素は添字ではなくキーで引く。重複キーや null キーの要素は実行時の Dictionary に
        /// 入らないので、シリアライズ上の添字と実行時の並びは一致しない。
        /// </summary>
        private static object ApplyDictionaryStep(
            IDictionary dictionary, SerializedObject serializedObject, PathStep entryStep, PathStep memberStep)
        {
            SerializedProperty keyProperty =
                serializedObject.FindProperty($"{entryStep.Path}.{DictionaryUtility.KEY_NAME}");
            if (keyProperty == null || DictionaryUtility.IsNullKey(keyProperty)) return null;

            Type keyType = dictionary.GetType().GetGenericArguments()[0];
            object key = ToKeyType(keyProperty.boxedValue, keyType);

            // 重複キーの2件目以降は実行時の Dictionary に入っていない。別要素の値を返さないよう、先頭の要素だけ認める
            int firstIndex = DictionaryUtility.IndexOfKey(
                serializedObject.FindProperty(PropertyPathUtility.GetParentPath(entryStep.Path)),
                DictionaryUtility.ToComparableKey(key));
            if (firstIndex != entryStep.Index || !dictionary.Contains(key)) return null;

            return string.Equals(memberStep.Name, DictionaryUtility.KEY_NAME, StringComparison.Ordinal)
                ? key
                : dictionary[key];
        }

        /// <summary>boxedValue の値を C# のキー型に揃える。enum キーの boxedValue は int で返るため。</summary>
        private static object ToKeyType(object value, Type keyType)
        {
            if (value == null) return null;
            if (keyType.IsEnum) return Enum.ToObject(keyType, value);
            if (keyType.IsPrimitive && value.GetType() != keyType) return Convert.ChangeType(value, keyType);

            return value;
        }

        private readonly struct PathStep
        {
            /// <summary>フィールド名。添字ステップなら null。</summary>
            public string Name { get; }

            /// <summary>配列添字。フィールド名ステップなら -1。</summary>
            public int Index { get; }

            /// <summary>このステップまでの propertyPath。</summary>
            public string Path { get; }

            public bool IsIndex => Index >= 0;

            public PathStep(string name, int index, string path)
            {
                Name = name;
                Index = index;
                Path = path;
            }
        }

        private static bool IsFrameworkNamespace(string namespaceName)
        {
            if (namespaceName == null) return false;

            return namespaceName.StartsWith("System", StringComparison.Ordinal)
                || namespaceName.StartsWith("Unity", StringComparison.Ordinal)
                || namespaceName.StartsWith("Microsoft", StringComparison.Ordinal);
        }
    }
}
