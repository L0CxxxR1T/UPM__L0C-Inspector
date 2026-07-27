// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>ネストしたクラス内の検証を確認するためのデータ。</summary>
    [Serializable]
    public sealed class NestedSampleData
    {
        // ネストしていても検証は再帰する（未設定なら親フィールドの下に "Data.Material" として出る）
        [Required]
        [SerializeField] private Material _material;

        // 段階1では装飾は再帰しないので、未対応であることが Warning で通知される
        [ReadOnly]
        [SerializeField] private string _note;
    }
}
