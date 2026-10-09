using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [TypeFilter] のドロップダウンに出す型の候補。[SerializeReference] と SerializableType で条件が違う。
    /// </summary>
    internal static class TypeCandidates
    {
        public const string NONE_CHOICE = "（なし）";

        private static readonly Dictionary<Type, List<Type>> _serializableTypeCache = new Dictionary<Type, List<Type>>();
        private static readonly Dictionary<Type, HashSet<string>> _serializableTypeNames =
            new Dictionary<Type, HashSet<string>>();

        /// <summary>
        /// [SerializeReference] に入れられる型。フィールドに代入でき、[Serializable] で、
        /// 引数なしで作れる具象クラスに限る（UnityEngine.Object は入れられない）。
        /// </summary>
        public static List<Type> ForManagedReference(Type fieldType, Type baseType)
        {
            List<Type> candidates = new List<Type>();
            foreach (Type type in EnumerateWithBase(baseType))
            {
                if (!IsConcrete(type) || typeof(Object).IsAssignableFrom(type)) continue;
                if (!type.IsSerializable || !fieldType.IsAssignableFrom(type)) continue;
                if (!type.IsValueType && !HasDefaultConstructor(type)) continue;

                candidates.Add(type);
            }

            SortByName(candidates);
            return candidates;
        }

        /// <summary>
        /// SerializableType で選べる型。基底型を継承した具象型ならよい（生成はしないので制約が少ない）。
        /// 検証ルールが更新のたびに呼ぶのでキャッシュする。ドメインリロードで作り直される。
        /// </summary>
        public static IReadOnlyList<Type> ForSerializableType(Type baseType)
        {
            if (_serializableTypeCache.TryGetValue(baseType, out List<Type> cached)) return cached;

            List<Type> candidates = new List<Type>();
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            foreach (Type type in EnumerateWithBase(baseType))
            {
                if (!IsConcrete(type) || type.FullName == null) continue;

                candidates.Add(type);
                names.Add(type.FullName);
            }

            SortByName(candidates);
            _serializableTypeCache.Add(baseType, candidates);
            _serializableTypeNames.Add(baseType, names);
            return candidates;
        }

        public static bool ContainsSerializableType(Type baseType, string typeName)
        {
            ForSerializableType(baseType);
            return _serializableTypeNames[baseType].Contains(typeName);
        }

        /// <summary>型名が重複するものだけ名前空間付きで表示する。</summary>
        public static List<string> BuildDisplayNames(IReadOnlyList<Type> candidates)
        {
            Dictionary<string, int> nameCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (Type type in candidates)
            {
                nameCounts.TryGetValue(type.Name, out int count);
                nameCounts[type.Name] = count + 1;
            }

            List<string> names = new List<string>();
            foreach (Type type in candidates)
            {
                names.Add(nameCounts[type.Name] > 1 ? type.FullName : type.Name);
            }

            return names;
        }

        private static IEnumerable<Type> EnumerateWithBase(Type baseType)
        {
            yield return baseType;
            foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType)) yield return type;
        }

        private static bool IsConcrete(Type type)
            => !type.IsAbstract && !type.IsInterface && !type.ContainsGenericParameters;

        private static bool HasDefaultConstructor(Type type)
            => type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null;

        private static void SortByName(List<Type> types)
            => types.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));
    }
}
