using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 数値の上限。インスペクタで上回る値を入れると上限に丸め、上回る値が保存されていれば検証層が Error を出す。
    /// 対象は整数 / 浮動小数 / Vector 系（成分ごと）と、それらの配列・List・Dictionary の値。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class MaxValueAttribute : Attribute
    {
        public double Max { get; }

        public MaxValueAttribute(double max) => Max = max;
    }
}
