using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// GameObject / Component 参照に、このコンポーネントの子孫だけを許す。それ以外は Error になる。
    /// Component にのみ使える。未設定は対象外（必須にしたいなら [Required] を併記する）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ChildGameObjectsOnlyAttribute : Attribute
    {
        /// <summary>自分自身の GameObject も許すか。</summary>
        public bool IncludeSelf { get; }

        public ChildGameObjectsOnlyAttribute(bool includeSelf = true) => IncludeSelf = includeSelf;
    }
}
