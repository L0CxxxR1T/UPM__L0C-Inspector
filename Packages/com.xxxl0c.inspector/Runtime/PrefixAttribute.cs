using System;

namespace XXXL0C.Inspector
{
    /// <summary>フィールドの前に短いテキストを添える。単位や記号を添えたいときに使う。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PrefixAttribute : Attribute
    {
        public string Text { get; }

        public PrefixAttribute(string text) => Text = text;
    }
}
