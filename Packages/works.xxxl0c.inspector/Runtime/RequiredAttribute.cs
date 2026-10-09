using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 未設定を検出させるマーカー。Object 参照 / [SerializeReference] / コレクション要素の null が対象。
    /// 空文字・空リストは「未設定」に含めない（誤検知が出ると警告全体が無視されるため）。
    /// </summary>
    /// <remarks>
    /// PropertyAttribute を継承しないのは意図的。この属性は PropertyDrawer ではなく中央 Editor が解釈する。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredAttribute : Attribute
    {
        /// <summary>未設定時に表示する文章。未指定なら既定の文章を使う。</summary>
        public string Message { get; }

        public RequiredAttribute(string message = null) => Message = message;
    }
}
