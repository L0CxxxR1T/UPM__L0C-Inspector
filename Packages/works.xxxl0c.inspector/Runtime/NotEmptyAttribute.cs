using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 空であることを検出する。対象は string / 配列 / List / Dictionary。
    /// 要素の null は対象外（[Required] の担当）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NotEmptyAttribute : Attribute
    {
        /// <summary>空のときに表示する文章。未指定なら既定の文章を使う。</summary>
        public string Message { get; }

        public NotEmptyAttribute(string message = null) => Message = message;
    }
}
