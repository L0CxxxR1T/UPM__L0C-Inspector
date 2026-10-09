using System;

namespace XXXL0C.Inspector
{
    /// <summary>Object 参照フィールドの右に、参照先のプレビュー画像を表示する。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PreviewFieldAttribute : Attribute
    {
        public const float DEFAULT_SIZE = 64f;

        /// <summary>プレビューの一辺の長さ（px）。</summary>
        public float Size { get; }

        public PreviewFieldAttribute(float size = DEFAULT_SIZE) => Size = size;
    }
}
