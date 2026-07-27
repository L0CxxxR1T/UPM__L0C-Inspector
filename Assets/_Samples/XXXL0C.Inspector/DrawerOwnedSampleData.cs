// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>
    /// 自前の PropertyDrawer を持つ型。中に装飾属性があっても**展開されない**ことの確認用。
    /// 他人製アセットの Drawer の描画を奪わないための安全装置が働いているかを見る。
    /// </summary>
    [Serializable]
    public sealed class DrawerOwnedSampleData
    {
        [Required]
        [SerializeField] private Material _material;

        // Drawer 持ちの型なので展開されず、この [ReadOnly] は反映されない旨が Warning で出る
        [ReadOnly]
        [SerializeField] private string _note = "Drawer 側で描画されます";
    }
}
