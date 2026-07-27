// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>個別 CustomEditor から静的ビルダーを呼ぶ例の対象。</summary>
    public sealed class CustomEditorSampleBehaviour : MonoBehaviour
    {
        [Required]
        [SerializeField] private Renderer _renderer;

        [SerializeField, Min(0f)] private float _radius = 1f;
    }
}
