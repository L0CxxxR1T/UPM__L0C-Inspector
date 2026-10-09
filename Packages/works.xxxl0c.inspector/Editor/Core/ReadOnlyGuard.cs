using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// フィールドを表示のみにする。SetEnabled(false) と違い、折りたたみの開閉とスクロールだけは通す。
    /// 編集につながる入力を TrickleDown で止めるので、ListView が後から作る要素にも効く。
    /// </summary>
    /// <remarks>
    /// [ReadOnly] と [ReadOnlyInPlayMode] が同じフィールドに付いても打ち消し合わないよう、
    /// ロックは掛けた側ごとに持ち、1つでも残っていれば表示のみにする。
    /// </remarks>
    internal static class ReadOnlyGuard
    {
        private const string COMMAND_COPY = "Copy";

        private static readonly ConditionalWeakTable<VisualElement, HashSet<object>> _locks =
            new ConditionalWeakTable<VisualElement, HashSet<object>>();

        /// <param name="owner">ロックを掛けた側を区別するキー。</param>
        public static void SetLocked(VisualElement element, object owner, bool locked)
        {
            if (!_locks.TryGetValue(element, out HashSet<object> owners))
            {
                owners = new HashSet<object>();
                _locks.Add(element, owners);
                RegisterBlockers(element, owners);
            }

            if (locked) owners.Add(owner);
            else owners.Remove(owner);

            element.EnableInClassList(InspectorClassNames.READ_ONLY, owners.Count > 0);
        }

        private static void RegisterBlockers(VisualElement root, HashSet<object> owners)
        {
            Block<PointerDownEvent>(root, owners);
            Block<MouseDownEvent>(root, owners);
            Block<ClickEvent>(root, owners);
            Block<KeyDownEvent>(root, owners);
            Block<NavigationSubmitEvent>(root, owners);
            Block<NavigationMoveEvent>(root, owners);
            Block<DragUpdatedEvent>(root, owners);
            Block<DragPerformEvent>(root, owners);
            Block<ContextClickEvent>(root, owners);
            Block<ContextualMenuPopulateEvent>(root, owners);

            // コピーだけは許す。貼り付け・削除・複製などは止める
            root.RegisterCallback<ValidateCommandEvent>(commandEvent =>
            {
                if (ShouldBlockCommand(commandEvent.commandName, commandEvent.target, root, owners)) Stop(commandEvent, root);
            }, TrickleDown.TrickleDown);
            root.RegisterCallback<ExecuteCommandEvent>(commandEvent =>
            {
                if (ShouldBlockCommand(commandEvent.commandName, commandEvent.target, root, owners)) Stop(commandEvent, root);
            }, TrickleDown.TrickleDown);
        }

        private static void Block<TEvent>(VisualElement root, HashSet<object> owners)
            where TEvent : EventBase<TEvent>, new()
        {
            root.RegisterCallback<TEvent>(evt =>
            {
                if (owners.Count == 0 || IsAllowedTarget(evt.target, root)) return;
                Stop(evt, root);
            }, TrickleDown.TrickleDown);
        }

        private static bool ShouldBlockCommand(
            string commandName, IEventHandler target, VisualElement root, HashSet<object> owners)
        {
            if (owners.Count == 0) return false;
            if (string.Equals(commandName, COMMAND_COPY, StringComparison.Ordinal)) return false;

            return !IsAllowedTarget(target, root);
        }

        private static void Stop(EventBase evt, VisualElement root)
        {
            evt.StopImmediatePropagation();

            // フォーカス移動などの既定動作も止める
            root.focusController?.IgnoreEvent(evt);
        }

        /// <summary>折りたたみの見出しとスクロールバーの上なら通す。</summary>
        private static bool IsAllowedTarget(IEventHandler target, VisualElement root)
        {
            for (VisualElement current = target as VisualElement; current != null; current = current.parent)
            {
                if (current.ClassListContains(Foldout.toggleUssClassName)) return true;
                if (current.ClassListContains(Scroller.ussClassName)) return true;
                if (current == root) break;
            }

            return false;
        }
    }
}
