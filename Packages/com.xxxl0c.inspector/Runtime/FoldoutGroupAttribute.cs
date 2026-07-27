using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 折りたためるグループにまとめる。並び順とパスの扱いは <see cref="BoxGroupAttribute"/> と同じ。
    /// </summary>
    /// <remarks>開閉状態は保持されるが、配下にエラーが出ても自動で開くことはしない。</remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FoldoutGroupAttribute : Attribute
    {
        /// <summary>グループのパス。"/" で区切ると入れ子になる。</summary>
        public string Path { get; }

        public FoldoutGroupAttribute(string path) => Path = path;
    }
}
