using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// ScriptableObject 全般のフォールバック。詳細は MonoBehaviourInspector と同じ。
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public sealed class ScriptableObjectInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI() => InspectorBuilder.Build(this);
    }
}
