using System;

namespace XXXL0C.Inspector
{
    /// <summary>Object 参照フィールドの下に、参照先のインスペクタをそのまま埋め込んで表示する。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class InlineEditorAttribute : Attribute
    {
        public bool ShowOpenButton { get; }

        public InlineEditorAttribute(bool showOpenButton = true) => ShowOpenButton = showOpenButton;
    }
}
