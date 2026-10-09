using System;
using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// enum の各メンバーに値を1つずつ持つ表。インスペクタでは enum の名前を見出しにして並ぶ。
    /// </summary>
    /// <remarks>
    /// Unity 6.5 以前向け。6.6 以降は標準でシリアライズされる Dictionary&lt;TEnum, TValue&gt; を使う方がよい。
    /// 値は enum の宣言順の位置で保存するので、メンバーを途中に挿入・並べ替えると値がずれる（末尾への追加は安全）。
    /// 値が飛び飛びの enum（1, 2, 4 など）にも使える。
    /// </remarks>
    [Serializable]
    public sealed class EnumIndexedList<TEnum, TValue> : ISerializationCallbackReceiver
        where TEnum : struct, Enum
    {
        [SerializeField] private List<TValue> _items = new List<TValue>();

        public EnumIndexedList() => EnsureSize();

        public int Count => EnumIndex<TEnum>.Count;

        public TValue this[TEnum key]
        {
            get => _items[EnumIndex<TEnum>.IndexOf(key)];
            set => _items[EnumIndex<TEnum>.IndexOf(key)] = value;
        }

        public void OnBeforeSerialize()
        {
        }

        /// <summary>enum に末尾のメンバーが増えた古いデータを、読み込んだ時点で埋める。</summary>
        public void OnAfterDeserialize() => EnsureSize();

        private void EnsureSize()
        {
            // enum からメンバーが減っても余りは消さない（戻したときに値が残るように）
            while (_items.Count < EnumIndex<TEnum>.Count) _items.Add(default);
        }

        /// <summary>enum の値から宣言順の位置を引く表。型ごとに1回だけ作る。</summary>
        private static class EnumIndex<T> where T : struct, Enum
        {
            private static readonly Dictionary<T, int> _indices = Build();

            public static int Count => _indices.Count;

            public static int IndexOf(T key)
            {
                if (_indices.TryGetValue(key, out int index)) return index;
                throw new ArgumentOutOfRangeException(nameof(key), key, $"{typeof(T).Name} に定義されていない値です。");
            }

            private static Dictionary<T, int> Build()
            {
                Dictionary<T, int> indices = new Dictionary<T, int>();
                foreach (T value in (T[])Enum.GetValues(typeof(T)))
                {
                    // 同じ値の別名は最初のメンバーにまとめる
                    if (!indices.ContainsKey(value)) indices.Add(value, indices.Count);
                }

                return indices;
            }
        }
    }
}
