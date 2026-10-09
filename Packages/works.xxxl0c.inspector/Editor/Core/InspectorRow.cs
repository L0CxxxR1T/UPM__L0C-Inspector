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
        private readonly List<ValidationMessage> _current = new List<ValidationMessage>();

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
