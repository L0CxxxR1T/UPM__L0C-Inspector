using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// Object 参照にシーン上のオブジェクトだけを許す。プレハブアセットや ScriptableObject などは Error になる。
    /// プレハブの中で自分の子を参照する場合は、インスタンス化するとシーン上のオブジェクトになるので許す。
    /// 未設定は対象外（必須にしたいなら [Required] を併記する）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SceneObjectsOnlyAttribute : Attribute
    {
    }
}
