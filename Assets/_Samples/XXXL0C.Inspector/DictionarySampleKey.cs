using System;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>DictionarySampleBehaviour の独自構造体キー。Dictionary のキーにするので等値性を実装する。</summary>
    [Serializable]
    public struct DictionarySampleKey : IEquatable<DictionarySampleKey>
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;

        public bool Equals(DictionarySampleKey other) => _row == other._row && _column == other._column;

        public override bool Equals(object obj) => obj is DictionarySampleKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_row, _column);
    }
}
