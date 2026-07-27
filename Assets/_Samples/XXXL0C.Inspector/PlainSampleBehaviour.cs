// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>
    /// 扱う属性が1つも無いクラス。標準インスペクタと同じ見た目で出ることの確認用。
    /// フォールバック Editor が他人製アセットを巻き込まないことの担保でもある。
    /// </summary>
    public sealed class PlainSampleBehaviour : MonoBehaviour
    {
        [Header("標準の属性のみ")]
        [Tooltip("説明文が出ること")]
        [SerializeField] private string _label = "サンプル";

        [SerializeField, Range(0f, 1f)] private float _ratio = 0.5f;

        [SerializeField, Min(0)] private int _amount;

        [SerializeField] private Vector3 _offset;

        [SerializeField] private GameObject _reference;

        [SerializeField] private List<int> _values = new List<int>();

        [TextArea]
        [SerializeField] private string _description;
    }
}
