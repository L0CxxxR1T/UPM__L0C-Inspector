// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>Unity 6.6 の標準 Dictionary シリアライズと、追加欄・検証の動作確認用。</summary>
    public sealed class DictionarySampleBehaviour : MonoBehaviour
    {
        // キーの重複は追加欄で弾かれる。値の null は [Required] で検出される
        [Header("文字列キー")]
        [Required]
        [SerializeField] private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

        [Header("enum キー")]
        [SerializeField] private Dictionary<DictionarySampleKind, float> _rates = new Dictionary<DictionarySampleKind, float>();

        // キーの参照切れも [Required] の対象
        [Header("Object キー")]
        [Required]
        [SerializeField] private Dictionary<Material, int> _materialOrders = new Dictionary<Material, int>();

        // 値のクラスの中の [Required] も検証される。装飾属性は反映されない旨が Warning で出る
        [Header("クラスの値")]
        [SerializeField] private Dictionary<int, NestedSampleData> _nested = new Dictionary<int, NestedSampleData>();

        // キーにも値にも「未設定」の概念が無いので、HelpBox で通知される
        [Header("[Required] が効かない型")]
        [Required]
        [SerializeField] private Dictionary<string, int> _counts = new Dictionary<string, int>();

        // 入力欄を用意できないキー型。既定値のキーで追加し、表の中で書き換える
        [Header("独自構造体のキー")]
        [SerializeField] private Dictionary<DictionarySampleKey, string> _cells = new Dictionary<DictionarySampleKey, string>();
    }
}
