using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using XXXL0C.Inspector.Editor;

namespace XXXL0C.Inspector.Samples.Editor
{
    /// <summary>
    /// 個別 CustomEditor から静的ビルダーを呼んで独自UIを足す例。
    /// 基底クラスの継承は不要で、Build() を呼ぶだけで属性機能が生きる。
    /// </summary>
    [CustomEditor(typeof(CustomEditorSampleBehaviour))]
    public sealed class CustomEditorSampleBehaviourEditor : UnityEditor.Editor
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = InspectorBuilder.Build(this);

            root.Add(new Label("↓ ここから下が個別 CustomEditor 側の独自UIです。"));
            root.Add(new Button(OnButtonClicked) { text = "独自ボタン" });

            return root;
        }

        private void OnButtonClicked() => Debug.Log($"{LOG_PREFIX}サンプルのボタンが押されました。");
    }
}
