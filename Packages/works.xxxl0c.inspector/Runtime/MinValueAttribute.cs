using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 数値の下限。インスペクタで下回る値を入れると下限に丸め、下回る値が保存されていれば検証層が Error を出す。
    /// 対象は整数 / 浮動小数 / Vector 系（成分ごと）と、それらの配列・List・Dictionary の値。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class MinValueAttribute : Attribute
    {
        public double Min { get; }

        public MinValueAttribute(double min) => Min = min;
    }
}
