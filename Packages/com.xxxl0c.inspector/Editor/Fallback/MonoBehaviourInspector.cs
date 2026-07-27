using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// MonoBehaviour 全般のフォールバック。
    /// 扱う属性を持たないクラスは InspectorBuilder が標準インスペクタに丸投げするので、
    /// 他人製アセットのコンポーネントの見た目は変わらない。
    /// </summary>
    /// <remarks>
    /// typeof(Object) まで広げないのは、Unity 標準アセットの描画を奪う事故を防ぐため。
    /// CanEditMultipleObjects は必須。付けないとプロジェクト内の全 MonoBehaviour で複数選択編集が壊れる。
    /// </remarks>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public sealed class MonoBehaviourInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI() => InspectorBuilder.Build(this);
    }
}
