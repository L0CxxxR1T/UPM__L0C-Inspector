using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// Edit Mode 中は表示のみにし、Play Mode 中だけ編集できるようにする。
    /// 一括チェックとビルド前フックは Edit Mode で走るので、このフィールドは検証されない。
    /// </summary>
    /// <remarks>逆（Play Mode 中だけ表示のみ）は <see cref="ReadOnlyInPlayModeAttribute"/> を使う。</remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class DisableInEditModeAttribute : Attribute
    {
    }
}
