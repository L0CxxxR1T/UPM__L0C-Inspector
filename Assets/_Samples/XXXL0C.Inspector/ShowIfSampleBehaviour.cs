// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>[ShowIf] / [HideIf] の動作確認用。</summary>
    public sealed class ShowIfSampleBehaviour : MonoBehaviour
    {
        public enum Mode
        {
            Simple,
            Custom,
            Advanced
        }

        [SerializeField] private bool _useCustomTarget;

        // bool 条件。非表示のあいだ [Required] の HelpBox は出ない
        [ShowIf(nameof(_useCustomTarget))]
        [Required]
        [SerializeField] private Transform _customTarget;

        [SerializeField] private Mode _mode;

        // enum 比較
        [ShowIf(nameof(_mode), Mode.Custom)]
        [SerializeField] private float _customWeight = 1f;

        [ShowIf(nameof(_mode), Mode.Advanced)]
        [SerializeField] private int _iterations = 4;

        // 反転
        [HideIf(nameof(_mode), Mode.Simple)]
        [SerializeField] private string _note;

        // 条件を複数付けると AND
        [ShowIf(nameof(_useCustomTarget))]
        [ShowIf(nameof(_mode), Mode.Custom)]
        [SerializeField] private Vector3 _customOffset;

        // わざと名前を間違えている。フィールドは見えたまま Warning が出るのが正しい挙動
        [ShowIf("_typoFieldName")]
        [SerializeField] private Color _tintOnTypo = Color.white;
    }
}
