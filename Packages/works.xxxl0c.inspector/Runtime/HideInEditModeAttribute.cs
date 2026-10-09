using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// Edit Mode 中はインスペクタに表示しない。
    /// 一括チェックとビルド前フックは Edit Mode で走るので、このフィールドは検証されない。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class HideInEditModeAttribute : Attribute
    {
    }
}
