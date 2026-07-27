using System;

namespace XXXL0C.Inspector
{
    /// <summary>float フィールドを、指定したステップに丸めるスライダーで表示する。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SteppedRangeAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get; }
        public float Step { get; }

        public SteppedRangeAttribute(float min, float max, float step)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
