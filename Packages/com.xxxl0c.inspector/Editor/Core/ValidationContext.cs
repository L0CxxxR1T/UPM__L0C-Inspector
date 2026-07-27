using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 検証ルールに渡す情報。ルールは渡された1プロパティだけを見る。
    /// 配列要素やネストしたクラスへの再帰は走査層（ValidationWalker）が担当する。
    /// </summary>
    public sealed class ValidationContext
    {
        private readonly List<ValidationMessage> _messages;

        public SerializedProperty Property { get; }

        /// <summary>属性の取得元。配列要素の場合は配列フィールド自身を指す。</summary>
        public FieldInfo FieldInfo { get; }

        /// <summary>この呼び出しの契機になった属性インスタンス。</summary>
        public Attribute Attribute { get; }

        /// <summary>メッセージ内でフィールドを指すための表示名。ネストしていれば相対パスになる。</summary>
        public string Label { get; }

        public ValidationContext(
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

        public void Report(ValidationSeverity severity, string text)
            => _messages.Add(new ValidationMessage(severity, text));
    }
}
