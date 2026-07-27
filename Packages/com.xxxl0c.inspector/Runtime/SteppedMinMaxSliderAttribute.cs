using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// min / max フィールドを持つ [Serializable] な値を、ステップ丸め付きの MinMax スライダーで表示する。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SteppedMinMaxSliderAttribute : Attribute
    {
        public float Min { get; }
        public float Max { get; }
        public float Step { get; }

        public SteppedMinMaxSliderAttribute(float min, float max, float step)
        {
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
