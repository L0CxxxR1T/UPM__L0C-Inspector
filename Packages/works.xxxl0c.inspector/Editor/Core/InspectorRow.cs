using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// インスペクタ1フィールド分の行。更新パスが触る対象。
    /// SerializedProperty は保持しない（Undo や再シリアライズで stale になるため）。
    /// </summary>
    internal sealed class InspectorRow
    {
        // [EnableIf] などで掛けるロックの持ち主。[ReadOnly] のロックと打ち消し合わないよう別のキーにする
        private static readonly object _enabledRuleLock = new object();

        private readonly List<ValidationMessage> _current = new List<ValidationMessage>();
        private bool _locked;

        public string PropertyPath { get; }
        public FieldInfo FieldInfo { get; }

        /// <summary>
        /// フィールドに付いた属性。更新パスは毎回の変更で走るので、リフレクションは構築時の1回だけにする。
        /// </summary>
        public Attribute[] Attributes { get; }

        /// <summary>メッセージ内でこのフィールドを指すための表示名。</summary>
        public string Label { get; }

        /// <summary>本体とメッセージ領域を含む行コンテナ。表示 / 非表示はここに掛ける。</summary>
        public VisualElement Root { get; }

        /// <summary>フィールドの本体。通常は PropertyField、展開したネストクラスでは Foldout。</summary>
        public VisualElement Field { get; }

        public VisualElement MessageArea { get; }

        public bool IsVisible { get; private set; } = true;

        /// <summary>表示されていて編集でき、親も同様であるか。false なら検証しない。</summary>
        public bool IsActive { get; private set; } = true;

        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }

        public InspectorRow(
            string propertyPath,
            FieldInfo fieldInfo,
            Attribute[] attributes,
            string label,
            VisualElement root,
            VisualElement field,
            VisualElement messageArea)
        {
            PropertyPath = propertyPath;
            FieldInfo = fieldInfo;
            Attributes = attributes;
            Label = label;
            Root = root;
            Field = field;
            MessageArea = messageArea;
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            Root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <param name="enabled">このフィールド自身のルールによる編集可否。親から引き継いだ分は含めない。</param>
        /// <param name="active">親も含めて表示・編集できる状態か。</param>
        public void SetEnabled(bool enabled, bool active)
        {
            IsActive = active;

            // 一度もロックしていない行には入力の監視を付けない（ReadOnlyGuard は初回に監視を登録する）
            if (enabled && !_locked) return;

            // 親が編集不可でも子には掛けない。親の本体がまとめて入力を止めるので、二重に掛けると見た目だけ濃くなる
            _locked = !enabled;
            ReadOnlyGuard.SetLocked(Field, _enabledRuleLock, _locked);
        }

        /// <summary>前回と同じ内容なら要素を作り直さない。</summary>
        public bool MatchesMessages(List<ValidationMessage> messages)
        {
            if (_current.Count != messages.Count) return false;

            for (int i = 0; i < messages.Count; i++)
            {
                if (!_current[i].Equals(messages[i])) return false;
            }

            return true;
        }

        public void ApplyMessages(List<ValidationMessage> messages)
        {
            _current.Clear();
            _current.AddRange(messages);

            ErrorCount = 0;
            WarningCount = 0;

            MessageArea.Clear();
            foreach (ValidationMessage message in messages)
            {
                if (message.Severity == ValidationSeverity.Error) ErrorCount++;
                else if (message.Severity == ValidationSeverity.Warning) WarningCount++;

                HelpBox helpBox = new HelpBox(message.Text, ToMessageType(message.Severity));
                helpBox.AddToClassList(InspectorClassNames.MESSAGE);
                MessageArea.Add(helpBox);
            }
        }

        private static HelpBoxMessageType ToMessageType(ValidationSeverity severity)
        {
            switch (severity)
            {
                case ValidationSeverity.Error:
                    return HelpBoxMessageType.Error;
                case ValidationSeverity.Warning:
                    return HelpBoxMessageType.Warning;
                default:
                    return HelpBoxMessageType.Info;
            }
        }
    }
}
