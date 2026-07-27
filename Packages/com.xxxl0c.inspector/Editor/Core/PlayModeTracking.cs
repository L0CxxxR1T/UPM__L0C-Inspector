using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 要素がパネルに接続されているあいだだけ EditorApplication.playModeStateChanged を購読し、
    /// Play Mode の切り替えに追従して再評価する。[ReadOnlyInPlayMode] と [Button] の
    /// EditorOnly / PlayModeOnly が共有する。
    /// </summary>
    internal static class PlayModeTracking
    {
        /// <summary>
        /// element がパネルに接続されるたびに reapply を呼び、Play Mode が切り替わるたびにも呼ぶ。
        /// 購読は接続中だけ生き、切断時に必ず解除する。
        /// </summary>
        public static void Track(VisualElement element, Action reapply)
        {
            void OnPlayModeStateChanged(PlayModeStateChange _) => reapply();

            element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                reapply();
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            });

            element.RegisterCallback<DetachFromPanelEvent>(_ =>
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged);

            reapply();
        }
    }
}
