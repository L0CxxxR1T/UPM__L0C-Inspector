using System;

namespace XXXL0C.Inspector
{
    /// <summary>Play Mode 中はインスペクタに表示しない。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class HideInPlayModeAttribute : Attribute
    {
    }
}
