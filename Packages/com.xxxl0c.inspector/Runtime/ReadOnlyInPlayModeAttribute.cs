using System;

namespace XXXL0C.Inspector
{
    /// <summary>Play Mode 中だけ編集を禁止し、表示のみにする。EditMode 中は通常どおり編集できる。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyInPlayModeAttribute : Attribute
    {
    }
}
