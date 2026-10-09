using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 条件を満たすときは表示のみにする。<see cref="EnableIfAttribute"/> の反転。
    /// 複数付けた場合はどれか1つ満たせば編集できなくなる。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class DisableIfAttribute : Attribute
    {
        /// <summary>条件に使うフィールドの名前。nameof で指定する。</summary>
        public string FieldName { get; }

        /// <summary>期待する値。null なら「その bool フィールドが true」を条件にする。</summary>
        public object ExpectedValue { get; }

        public DisableIfAttribute(string fieldName) : this(fieldName, null)
        {
        }

        public DisableIfAttribute(string fieldName, object expectedValue)
        {
            FieldName = fieldName;
            ExpectedValue = expectedValue;
        }
    }
}
