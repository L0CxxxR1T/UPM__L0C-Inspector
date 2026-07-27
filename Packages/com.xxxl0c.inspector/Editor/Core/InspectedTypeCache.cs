using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 「この型に扱う属性が付いているか」を型単位でキャッシュする。
    /// 早期 return の判定と、検証の再帰の枝刈りの両方に使う。
    /// </summary>
    /// <remarks>
    /// 判定は必ず過剰包含側に倒す。true を返しすぎても遅い経路を通るだけだが、
    /// false を返し間違えると機能が黙って死ぬ。
    /// </remarks>
    internal static class InspectedTypeCache
    {
        private const int MAX_DEPTH = 10;

        private static readonly Dictionary<Type, bool> _cache = new Dictionary<Type, bool>();

        /// <summary>
        /// その型（ネストした [Serializable] クラスやコレクション要素まで含む）に、
        /// 扱う属性が1つでも付いているか。
        /// </summary>
        public static bool ContainsHandledAttributes(Type type)
        {
            if (type == null) return false;
            return Scan(type, 0, new HashSet<Type>());
        }

        /// <summary>そのフィールドに、扱う属性が1つでも付いているか。</summary>
        public static bool HasHandledAttribute(FieldInfo field)
        {
            if (field == null) return false;

            foreach (object candidate in field.GetCustomAttributes(false))
            {
                if (candidate is Attribute attribute && ExtensionRegistry.IsHandled(attribute.GetType()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Scan(Type type, int depth, HashSet<Type> visiting)
        {
            if (_cache.TryGetValue(type, out bool cached)) return cached;

            // 打ち切りと循環は安全側（true）に倒す。ここでは結果をキャッシュしない
            if (depth > MAX_DEPTH) return true;
            if (!visiting.Add(type)) return true;

            bool found = false;
            foreach (FieldInfo field in SerializedFieldUtility.EnumerateSerializedFields(type))
            {
                if (HasHandledAttribute(field))
                {
                    found = true;
                    break;
                }

                // [SerializeReference] は実際に入る型が静的に分からないので調べ切れない。安全側に倒す
                if (field.IsDefined(typeof(SerializeReference), false))
                {
                    found = true;
                    break;
                }

                Type inner = SerializedFieldUtility.GetCollectionElementType(field.FieldType) ?? field.FieldType;
                if (!SerializedFieldUtility.CanContainSerializedFields(inner)) continue;

                if (Scan(inner, depth + 1, visiting))
                {
                    found = true;
                    break;
                }
            }

            visiting.Remove(type);
            _cache[type] = found;
            return found;
        }
    }
}
