using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 枠で囲んだグループにまとめる。
    /// グループは「そのグループに属する最初のフィールドが現れた位置」に出るので、
    /// 並び順はフィールドの宣言順で制御する（順序引数は持たない）。
    /// </summary>
    /// <remarks>
    /// グループは見た目の話でしかない。シリアライズ構造やクラス設計をグループ都合で変えないこと。
    /// 表示条件はグループ側ではなくフィールド側に [ShowIf] で1箇所だけ書く。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class BoxGroupAttribute : Attribute
    {
        /// <summary>グループのパス。"/" で区切ると入れ子になる。</summary>
        public string Path { get; }

        public BoxGroupAttribute(string path) => Path = path;
    }
}
