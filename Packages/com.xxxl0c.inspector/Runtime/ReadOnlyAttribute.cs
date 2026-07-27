using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// インスペクタでの編集を禁止し、表示のみにするマーカー。
    /// </summary>
    /// <remarks>
    /// PropertyAttribute を継承しないのは意図的。この属性は PropertyDrawer ではなく中央 Editor が解釈する。
    /// System.ComponentModel.ReadOnlyAttribute と同名なので、使う側で System.ComponentModel を using しないこと。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }
}
