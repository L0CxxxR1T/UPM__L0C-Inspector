using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>可視性ルールに渡す情報。</summary>
    public sealed class VisibilityContext
    {
        private readonly List<ValidationMessage> _messages;

        public SerializedProperty Property { get; }
        public FieldInfo FieldInfo { get; }

        /// <summary>この呼び出しの契機になった属性インスタンス。</summary>
        public Attribute Attribute { get; }

        /// <summary>メッセージ内でフィールドを指すための表示名。</summary>
        public string Label { get; }

        public VisibilityContext(
            SerializedProperty property,
            FieldInfo fieldInfo,
            Attribute attribute,
            string label,
            List<ValidationMessage> messages)
        {
            Property = property;
            FieldInfo = fieldInfo;
            Attribute = attribute;
            Label = label;
            _messages = messages;
        }

        /// <summary>
        /// 条件を評価できないなどの誤用を報告する。検証と同じメッセージ列に流れる。
        /// 黙って無視しないための経路なので、判定を諦めるときは必ず報告すること。
        /// </summary>
        public void Report(ValidationSeverity severity, string text)
            => _messages.Add(new ValidationMessage(severity, text));
    }
}
