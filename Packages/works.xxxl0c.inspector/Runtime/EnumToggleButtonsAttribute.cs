using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// enum フィールドをドロップダウンではなく横並びのボタンで表示する。
    /// [Flags] の enum は複数選択できる（0 と、複数ビットを合わせた値はボタンにしない）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class EnumToggleButtonsAttribute : Attribute
    {
    }
}
