using UnityEngine;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[SteppedRange] / [SteppedMinMaxSlider] が共有するステップ丸め計算。</summary>
    internal static class SteppedValueUtility
    {
        public static float Snap(float value, float min, float max, float step)
        {
            if (step <= 0f) return Mathf.Clamp(value, min, max);

            float snapped = min + Mathf.Round((value - min) / step) * step;
            return Mathf.Clamp(snapped, min, max);
        }
    }
}
