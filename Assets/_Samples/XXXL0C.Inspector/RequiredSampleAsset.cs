// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>ScriptableObject 側でも同じように動くことの確認用。</summary>
    [CreateAssetMenu(
        menuName = "XXXL0C/Samples/Required Sample Asset",
        fileName = "RequiredSampleAsset")]
    public sealed class RequiredSampleAsset : ScriptableObject
    {
        [Required]
        [SerializeField] private Material _material;

        [ReadOnly]
        [SerializeField] private Texture2D _lockedTexture;

        [SerializeField] private NestedSampleData _data;
    }
}
