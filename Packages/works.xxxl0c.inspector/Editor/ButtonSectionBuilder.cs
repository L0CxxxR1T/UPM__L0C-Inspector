using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [Button] 付きメソッドをインスペクタのボタンとして組み立てる。
    /// フィールド前提の5拡張点（装飾・可視性・検証・グループ・本体差し替え）とは別軸の仕組みで、
    /// 属性が1つしかないので TypeCache 経由の拡張点は作らずここに直接書く。
    /// </summary>
    internal static class ButtonSectionBuilder
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";
        private const string TOOLTIP_EDITOR_ONLY = "Edit Mode 中のみ実行できます。";
        private const string TOOLTIP_PLAY_MODE_ONLY = "Play Mode 中のみ実行できます。";

        private static readonly FoldoutGroupFactory _defaultGroupFactory = new FoldoutGroupFactory();

        /// <summary>
        /// targetType の [Button] メソッドをすべて組み立てて配置する。
        /// グループ指定が無ければ rootNode（フッター）に、あればそのパスのグループに入れる。
        /// </summary>
        public static void Build(
            SerializedObject serializedObject, Type targetType, GroupNode rootNode, GroupTree groupTree)
        {
            foreach (MethodInfo method in ButtonMethodCache.GetButtonMethods(targetType))
            {
                ButtonAttribute attribute = method.GetCustomAttribute<ButtonAttribute>();
                if (attribute == null) continue;

                GroupNode ownerNode = string.IsNullOrEmpty(attribute.Group)
                    ? rootNode
                    : groupTree.Resolve(rootNode, attribute.Group, _defaultGroupFactory);

                AddButton(ownerNode, serializedObject, method, attribute);
            }
        }

        private static void AddButton(
            GroupNode ownerNode, SerializedObject serializedObject, MethodInfo method, ButtonAttribute attribute)
        {
            // 常時表示のコンテンツを追加したことを伝える。伝えないと「行が無い空グループ」として隠される
            ownerNode.MarkHasStaticContent();

            if (method.GetParameters().Length > 0)
            {
                HelpBox notice = new HelpBox(
                    $"[Button] は引数なしメソッドにのみ使えます（{method.Name}）。", HelpBoxMessageType.Warning);
                notice.AddToClassList(InspectorClassNames.NOTICE);
                ownerNode.Content.Add(notice);
                return;
            }

            string label = string.IsNullOrEmpty(attribute.Label)
                ? ObjectNames.NicifyVariableName(method.Name)
                : attribute.Label;

            Button button = new Button(() => Invoke(serializedObject, method, label)) { text = label };
            button.AddToClassList(InspectorClassNames.BUTTON);

            if (attribute.Mode != ButtonMode.Always)
            {
                PlayModeTracking.Track(button, () => ApplyModeState(button, attribute.Mode));
            }

            ownerNode.Content.Add(button);
        }

        private static void ApplyModeState(Button button, ButtonMode mode)
        {
            bool matches = mode == ButtonMode.PlayModeOnly
                ? EditorApplication.isPlaying
                : !EditorApplication.isPlaying;

            button.SetEnabled(matches);
            button.tooltip = matches
                ? string.Empty
                : mode == ButtonMode.PlayModeOnly ? TOOLTIP_PLAY_MODE_ONLY : TOOLTIP_EDITOR_ONLY;
        }

        /// <summary>
        /// 選択されている全オブジェクトに対してメソッドを呼ぶ。1回のクリックで1つの Undo グループにまとめる。
        /// 1つの呼び出しが例外を投げても、他のオブジェクトへの呼び出しは続ける（黙って握り潰しはしない、
        /// ログには残す）。
        /// </summary>
        private static void Invoke(SerializedObject serializedObject, MethodInfo method, string label)
        {
            Object[] targets = serializedObject.targetObjects;

            // 新しいグループを切らないと、直前の編集と同じ Undo にまとめられてしまう
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Button: {label}");

            foreach (Object target in targets)
            {
                Undo.RecordObject(target, label);

                try
                {
                    method.Invoke(target, null);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, target);
                }

                EditorUtility.SetDirty(target);
            }

            Undo.CollapseUndoOperations(group);
        }
    }
}
