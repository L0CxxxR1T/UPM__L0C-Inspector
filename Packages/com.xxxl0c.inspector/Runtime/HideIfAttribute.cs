using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 条件を満たすとインスペクタから隠す。[ShowIf] の反転で、条件の書き方は同じ。
    /// 複数付けた場合はどれか1つでも満たせば隠れる。
    /// </summary>
    /// <remarks>条件対象はフィールド限定で、指定は nameof を使う。理由は <see cref="ShowIfAttribute"/> を参照。</remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class HideIfAttribute : Attribute
    {
        /// <summary>条件に使うフィールドの名前。nameof で指定する。</summary>
        public string FieldName { get; }

        /// <summary>期待する値。null なら「その bool フィールドが true」を条件にする。</summary>
        public object ExpectedValue { get; }

        public HideIfAttribute(string fieldName) : this(fieldName, null)
        {
        }

        public HideIfAttribute(string fieldName, object expectedValue)
        {
            FieldName = fieldName;
            ExpectedValue = expectedValue;
        }
    }
}
