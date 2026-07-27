// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>MonoBehaviour 側の [Required] / [ReadOnly] 動作確認用。</summary>
    public sealed class RequiredSampleBehaviour : MonoBehaviour
    {
        [Header("Object 参照")]
        [Required]
        [SerializeField] private GameObject _target;

        [Required("参照先を指定してください。")]
        [SerializeField] private Transform _customMessageTarget;

        // 併記の確認。ReadOnly で編集できないので Required の警告は出たままになるが、
        // それが「両方の属性が適用されている」証拠になる
        [Header("属性の併記")]
        [Required]
        [ReadOnly]
        [SerializeField] private Renderer _lockedTarget;

        // 「未設定」の概念が無い型。黙って無視せず HelpBox で通知する
        [Header("効果が無い型に付けた場合")]
        [Required]
        [SerializeField] private int _count;

        // 要素の null が検出される。空リストは対象外
        [Header("コレクション")]
        [Required]
        [SerializeField] private List<GameObject> _items = new List<GameObject>();

        [Header("ネストしたクラス")]
        [SerializeField] private NestedSampleData _data;

        [Header("属性なし（通常表示）")]
        [SerializeField, Range(0f, 1f)] private float _ratio = 0.5f;
    }
}
