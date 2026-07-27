// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>
    /// IFieldFactory の辞退（型違い）を確認するためのサンプル。
    /// いずれも標準の PropertyField にフォールバックし、辞退理由が notice として出ること。
    /// </summary>
    public sealed class MisusedAttributeSampleBehaviour : MonoBehaviour
    {
        [SteppedRange(0f, 1f, 0.1f)]
        [SerializeField] private string _wrongTypeForSteppedRange;

        [SceneName]
        [SerializeField] private int _wrongTypeForSceneName;

        [SteppedMinMaxSlider(0f, 1f, 0.1f)]
        [SerializeField] private int _noMinMaxFields;

        // 本体を差し替える属性2つの併記。先着（型名順）が使われ LogError が出る
        [SteppedRange(0f, 1f, 0.1f)]
        [SceneName]
        [SerializeField] private string _conflictingFactories;
    }
}
