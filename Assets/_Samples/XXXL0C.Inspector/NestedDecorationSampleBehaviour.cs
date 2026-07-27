// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>ネストしたクラス内の装飾と検証の確認用。</summary>
    public sealed class NestedDecorationSampleBehaviour : MonoBehaviour
    {
        // 自前展開されるので、中の [ReadOnly] が実際に効く（段階1 の Warning は出なくなる）
        [SerializeField] private NestedSampleData _expanded;

        // Drawer を持つ型なので展開されず、中の [ReadOnly] は Warning で未反映が通知される
        [SerializeField] private DrawerOwnedSampleData _drawerOwned;

        // 展開したネストクラスの中でもグループが使える
        [FoldoutGroup("ネスト")]
        [SerializeField] private NestedSampleData _grouped;
    }
}
