// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>
    /// [SteppedMinMaxSlider] 用。min / max フィールドを持つ [Serializable] 型。
    /// フィールド名は本来 `_camelCase` だが、SteppedMinMaxSliderFieldFactory は
    /// 素の `min` / `max`（旧ライブラリの契約）にもフォールバックするので、その経路も兼ねて確認する
    /// （通常の命名 `_min` / `_max` は SteppedMinMaxSliderFieldFactory 側の第一候補）。
    /// </summary>
    [Serializable]
    public struct SteppedRangeSampleData
    {
        [SerializeField] private float _min;
        [SerializeField] private float _max;
    }
}
