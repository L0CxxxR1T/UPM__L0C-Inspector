using System;
using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// <see cref="SerializableType"/> の名前から Type を引くための明示的な登録表。
    /// 起動時（Composition Root など）に、実行時に引く可能性のある型をすべて登録する。
    /// </summary>
    /// <example>
    /// <code>
    /// SerializableTypeRegistry.Register&lt;SlimeEnemy&gt;();
    /// SerializableTypeRegistry.Register&lt;GoblinEnemy&gt;();
    /// Type enemyType = _enemyType.Resolve();
    /// </code>
    /// </example>
    public static class SerializableTypeRegistry
    {
        private static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>(StringComparer.Ordinal);

        public static void Register<T>() => Register(typeof(T));

        /// <summary>
        /// 型を登録する。同じ型の再登録は何もしない。完全名が同じ別の型（別アセンブリ）は区別できないので例外を投げる。
        /// </summary>
        public static void Register(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (_types.TryGetValue(type.FullName, out Type existing))
            {
                if (existing == type) return;

                throw new InvalidOperationException(
                    $"完全名 '{type.FullName}' の型が複数のアセンブリにあるため登録できません"
                    + $"（{existing.Assembly.GetName().Name} / {type.Assembly.GetName().Name}）。");
            }

            _types.Add(type.FullName, type);
        }

        public static bool TryGet(string typeName, out Type type)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                type = null;
                return false;
            }

            return _types.TryGetValue(typeName, out type);
        }

        /// <summary>ドメインリロード無しで Play Mode に入ったとき、前回の登録を持ち越さない。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => _types.Clear();

        /// <summary>テスト用。登録をすべて消す。</summary>
        internal static void Clear() => _types.Clear();
    }
}
