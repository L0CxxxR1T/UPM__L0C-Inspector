using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>検証ルールが返す1件のメッセージ。</summary>
    public readonly struct ValidationMessage : IEquatable<ValidationMessage>
    {
        public ValidationSeverity Severity { get; }
        public string Text { get; }

        public ValidationMessage(ValidationSeverity severity, string text)
        {
            Severity = severity;
            Text = text;
        }

        public bool Equals(ValidationMessage other)
            => Severity == other.Severity && string.Equals(Text, other.Text, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is ValidationMessage other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Severity, Text);
    }
}
