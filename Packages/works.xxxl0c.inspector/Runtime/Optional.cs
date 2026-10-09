using System;
using UnityEngine;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 「値が無い」状態を持てる値。Unity のシリアライズは null と既定値を区別できないので、
    /// 有無を bool で明示的に持つ。インスペクタではチェックボックスと値の欄が1行に並ぶ。
    /// </summary>
    [Serializable]
    public struct Optional<T>
    {
        [SerializeField] private bool _hasValue;
        [SerializeField] private T _value;

        public bool HasValue => _hasValue;

        /// <summary>値を返す。値が無いときに読むのはバグなので例外を投げる。</summary>
        public T Value
        {
            get
            {
                if (!_hasValue) throw new InvalidOperationException($"Optional<{typeof(T).Name}> に値がありません。");
                return _value;
            }
        }

        public Optional(T value)
        {
            _hasValue = true;
            _value = value;
        }

        public static Optional<T> Some(T value) => new Optional<T>(value);

        public static Optional<T> None() => default;

        public bool TryGetValue(out T value)
        {
            value = _hasValue ? _value : default;
            return _hasValue;
        }

        public T GetValueOrDefault(T fallback = default) => _hasValue ? _value : fallback;

        public override string ToString() => _hasValue ? $"Some({_value})" : "None";
    }
}
