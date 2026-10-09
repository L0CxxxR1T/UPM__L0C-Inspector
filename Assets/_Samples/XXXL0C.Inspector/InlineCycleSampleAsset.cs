// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>
    /// [InlineEditor] の循環参照の確認用。A と B が互いを参照していても、埋め込みは1周で止まる。
    /// </summary>
    [CreateAssetMenu(
        menuName = "XXXL0C/Samples/Inline Cycle Sample Asset",
        fileName = "InlineCycleSampleAsset")]
    public sealed class InlineCycleSampleAsset : ScriptableObject
    {
        [SerializeField] private string _memo;

        [InlineEditor]
        [SerializeField] private InlineCycleSampleAsset _partner;
    }
}
