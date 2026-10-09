using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 同じグループのフィールドを横に並べる。各フィールドは等幅になり、ラベルは中身に合わせて縮む。
    /// "/" で区切ると他のグループの中に入れられる。
    /// </summary>
    /// <remarks>グループは見た目の話でしかない。シリアライズ構造やクラス設計をグループ都合で変えないこと。</remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class HorizontalGroupAttribute : Attribute
    {
        /// <summary>グループのパス。"/" で区切ると入れ子になる。</summary>
        public string Path { get; }

        public HorizontalGroupAttribute(string path) => Path = path;
    }
}
