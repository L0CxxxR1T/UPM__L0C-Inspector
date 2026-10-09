using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// Object 参照にアセットだけを許す。シーン上のオブジェクトや、プレハブ内の自分の子への参照は Error になる。
    /// 未設定は対象外（必須にしたいなら [Required] を併記する）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class AssetsOnlyAttribute : Attribute
    {
    }
}
