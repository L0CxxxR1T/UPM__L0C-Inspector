using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 条件を満たすときだけインスペクタに表示する。複数付けた場合は全て満たしたときだけ表示する（AND）。
    /// 非表示のフィールドは [Required] の対象外になる（設定できないものを未設定だと責めない）。
    /// </summary>
    /// <remarks>
    /// 条件対象はフィールド限定で、指定は nameof を使う。メソッドやプロパティを条件にしないのは、
    /// ビルド前バリデータが条件を評価する際にアセット上でユーザーコードを実行することになり、
    /// 副作用があると事故るため。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class ShowIfAttribute : Attribute
    {
        /// <summary>条件に使うフィールドの名前。nameof で指定する。</summary>
        public string FieldName { get; }

        /// <summary>期待する値。null なら「その bool フィールドが true」を条件にする。</summary>
        public object ExpectedValue { get; }

        public ShowIfAttribute(string fieldName) : this(fieldName, null)
        {
        }

        public ShowIfAttribute(string fieldName, object expectedValue)
        {
            FieldName = fieldName;
            ExpectedValue = expectedValue;
        }
    }
}
