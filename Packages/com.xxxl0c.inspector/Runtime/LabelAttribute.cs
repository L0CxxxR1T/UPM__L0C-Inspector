using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// インスペクタ上のラベルを差し替える。フィールド名を変えずに表示名だけ変えたいときに使う。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LabelAttribute : Attribute
    {
        public string DisplayName { get; }

        public LabelAttribute(string displayName) => DisplayName = displayName;
    }
}
