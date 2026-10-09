using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// Dictionary フィールドの本体。表示は Unity 標準の Drawer に任せ、追加だけを自前の追加欄に一本化する。
    /// 標準の「+」と複製・貼り付けは選択中の要素を複製するため、押した時点で重複キーができてしまう。
    /// </summary>
    /// <remarks>
    /// 標準の「+」は ListView の公開名で探して隠す。Unity 側の実装が変わって見つからなくなった場合は、
    /// 警告を出して標準の動作に戻る（重複キーは検証層が検出する）。
    /// </remarks>
    internal sealed class DictionaryField : VisualElement
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";
        private const string DICTIONARY_VIEW_CLASS = "unity-dictionary-view";

        // Unity のコマンド名（EventCommandNames は internal なので文字列で持つ）
        private const string COMMAND_DUPLICATE = "Duplicate";
        private const string COMMAND_PASTE = "Paste";

        private static bool _missingAddButtonLogged;

        private readonly DictionaryAddPanel _addPanel;
        private Foldout _trackedFoldout;

        /// <summary>標準の Dictionary 表示を担う PropertyField。装飾はこれに対して行う。</summary>
        public PropertyField PropertyField { get; }

        public DictionaryField(SerializedProperty property, Type keyType, Type valueType)
        {
            AddToClassList(InspectorClassNames.DICTIONARY_FIELD);

            PropertyField = new PropertyField(property);
            Add(PropertyField);

            // 複数選択時は標準の表示自体が編集できない旨の HelpBox になるので、追加欄も出さない
            if (property.serializedObject.isEditingMultipleObjects) return;

            _addPanel = new DictionaryAddPanel(property, keyType, valueType);
            Add(_addPanel);

            PropertyField.RegisterCallback<GeometryChangedEvent>(_ => SyncWithNativeView());
            PropertyField.RegisterCallback<ValidateCommandEvent>(OnValidateCommand, TrickleDown.TrickleDown);
            PropertyField.RegisterCallback<ExecuteCommandEvent>(OnExecuteCommand, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// 標準の表示に合わせる。PropertyField は中身を後から組み立て、再バインドで作り直すこともあるので
        /// レイアウトが変わるたびに確かめる。
        /// </summary>
        private void SyncWithNativeView()
        {
            VisualElement view = PropertyField.Q(className: DICTIONARY_VIEW_CLASS);
            if (view == null) return;

            HideNativeAddButtons(view);
            TrackFoldout(view);
        }

        /// <summary>追加欄は標準の表示を折りたたんだら一緒に隠す。</summary>
        private void TrackFoldout(VisualElement view)
        {
            Foldout foldout = view.Query<Foldout>(className: BaseListView.foldoutHeaderUssClassName)
                .Where(candidate => FindOwnerView(candidate) == view)
                .First();
            if (foldout == null || foldout == _trackedFoldout) return;

            _trackedFoldout = foldout;
            foldout.RegisterValueChangedCallback(changeEvent =>
            {
                // 入れ子の Toggle などから伝わってきた変更は無視する
                if (changeEvent.target != foldout) return;

                ApplyFoldoutState(changeEvent.newValue);
            });
            ApplyFoldoutState(foldout.value);
        }

        private void ApplyFoldoutState(bool expanded)
            => _addPanel.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

        /// <summary>標準の「+」を隠す。入れ子の Dictionary の「+」は追加欄が無いので残す。</summary>
        private void HideNativeAddButtons(VisualElement view)
        {
            List<Button> buttons = view.Query<Button>(BaseListView.footerAddButtonName).ToList();
            bool found = false;
            foreach (Button button in buttons)
            {
                if (FindOwnerView(button) != view) continue;

                found = true;
                button.style.display = DisplayStyle.None;
            }

            if (found || _missingAddButtonLogged) return;

            _missingAddButtonLogged = true;
            Debug.LogWarning(
                $"{LOG_PREFIX}Dictionary の標準の追加ボタンが見つからないため、非表示にできませんでした。"
                + "標準の追加ボタンからは重複キーを作れますが、検証で検出されます。");
        }

        private static VisualElement FindOwnerView(VisualElement element)
        {
            for (VisualElement current = element.parent; current != null; current = current.parent)
            {
                if (current.ClassListContains(DICTIONARY_VIEW_CLASS)) return current;
            }

            return null;
        }

        private static void OnValidateCommand(ValidateCommandEvent commandEvent)
        {
            if (ShouldBlock(commandEvent.commandName, commandEvent.target)) commandEvent.StopImmediatePropagation();
        }

        private static void OnExecuteCommand(ExecuteCommandEvent commandEvent)
        {
            if (ShouldBlock(commandEvent.commandName, commandEvent.target)) commandEvent.StopImmediatePropagation();
        }

        /// <summary>
        /// 要素を複製するコマンドを止める。貼り付けはテキスト入力中なら文字列の貼り付けなので通す。
        /// </summary>
        private static bool ShouldBlock(string commandName, IEventHandler target)
        {
            if (string.Equals(commandName, COMMAND_DUPLICATE, StringComparison.Ordinal)) return true;
            if (!string.Equals(commandName, COMMAND_PASTE, StringComparison.Ordinal)) return false;

            return !(target is VisualElement element) || !IsInsideTextInput(element);
        }

        private static bool IsInsideTextInput(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                for (Type type = current.GetType(); type != null; type = type.BaseType)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TextInputBaseField<>)) return true;
                }
            }

            return false;
        }
    }
}
