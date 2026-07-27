using System;

namespace XXXL0C.Inspector
{
    /// <summary>string フィールドを、Build Settings に登録されたシーン名のドロップダウンで表示する。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SceneNameAttribute : Attribute
    {
    }
}
