using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 条件を満たすときだけ編集できるようにする。満たさないときは表示のみになる。
    /// 複数付けた場合は全て満たしたときだけ編集できる（AND）。
    /// 編集できないフィールドは [Required] などの検証の対象外になる（設定できないものを未設定だと責めない）。
    /// </summary>
    /// <remarks>条件の指定方法は <see cref="ShowIfAttribute"/> と同じ（フィールド限定、nameof で指定）。</remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class EnableIfAttribute : Attribute
    {
        /// <summary>条件に使うフィールドの名前。nameof で指定する。</summary>
        public string FieldName { get; }

        /// <summary>期待する値。null なら「その bool フィールドが true」を条件にする。</summary>
        public object ExpectedValue { get; }

        public EnableIfAttribute(string fieldName) : this(fieldName, null)
        {
        }

        public EnableIfAttribute(string fieldName, object expectedValue)
        {
            FieldName = fieldName;
            ExpectedValue = expectedValue;
        }
    }
}
