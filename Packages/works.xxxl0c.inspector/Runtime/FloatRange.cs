using System;
using UnityEngine;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// float の最小値と最大値の組。[SteppedMinMaxSlider] でそのまま表示できる。
    /// </summary>
    [Serializable]
    public struct FloatRange
    {
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        public float Min => _min;
        public float Max => _max;
        public float Length => _max - _min;

        public FloatRange(float min, float max)
        {
            if (min > max) throw new ArgumentException($"min（{min}）が max（{max}）より大きくなっています。");

            _min = min;
            _max = max;
        }

        public bool Contains(float value) => value >= _min && value <= _max;

        public float Clamp(float value) => Mathf.Clamp(value, _min, _max);

        /// <summary>t = 0 で Min、t = 1 で Max。t は 0〜1 に丸める。</summary>
        public float Lerp(float t) => Mathf.Lerp(_min, _max, t);

        public override string ToString() => $"[{_min}, {_max}]";
    }
}
