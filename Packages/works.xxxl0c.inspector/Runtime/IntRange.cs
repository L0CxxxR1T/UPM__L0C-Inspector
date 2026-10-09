using System;
using UnityEngine;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// int の最小値と最大値の組（両端を含む）。[SteppedMinMaxSlider] でそのまま表示できる。
    /// </summary>
    [Serializable]
    public struct IntRange
    {
        [SerializeField] private int _min;
        [SerializeField] private int _max;

        public int Min => _min;
        public int Max => _max;

        /// <summary>範囲に含まれる整数の個数。</summary>
        public int Count => _max - _min + 1;

        public IntRange(int min, int max)
        {
            if (min > max) throw new ArgumentException($"min（{min}）が max（{max}）より大きくなっています。");

            _min = min;
            _max = max;
        }

        public bool Contains(int value) => value >= _min && value <= _max;

        public int Clamp(int value) => Mathf.Clamp(value, _min, _max);

        public override string ToString() => $"[{_min}, {_max}]";
    }
}
