using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// タブにまとめる。同じ group を指定したフィールドが1つのタブ群になり、tab ごとにページが分かれる。
    /// タブの並びは、そのタブに属する最初のフィールドの宣言順で決まる。
    /// </summary>
    /// <remarks>
    /// group に "/" を含めると他のグループの中に置ける（外側のグループは先に宣言しておく）。
    /// ページの中にさらにグループを作るときは、他のグループ属性に "タブ群/タブ/内側" のパスを指定する。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TabGroupAttribute : Attribute
    {
        /// <summary>タブ群のパス。</summary>
        public string Group { get; }

        /// <summary>タブの名前。</summary>
        public string Tab { get; }

        public TabGroupAttribute(string group, string tab)
        {
            Group = group;
            Tab = tab;
        }
    }
}
