using System;

namespace XXXL0C.Inspector
{
    /// <summary>フィールドの後ろに短いテキストを添える。単位や記号を添えたいときに使う。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SuffixAttribute : Attribute
    {
        public string Text { get; }

        public SuffixAttribute(string text) => Text = text;
    }
}
