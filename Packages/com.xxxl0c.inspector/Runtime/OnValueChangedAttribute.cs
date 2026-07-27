using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 値が変わるたびに、同じインスタンス上の引数なしメソッドを呼ぶ。
    /// メソッド名は nameof で指定する。存在しない場合は検証層が警告する。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class OnValueChangedAttribute : Attribute
    {
        public string MethodName { get; }

        public OnValueChangedAttribute(string methodName) => MethodName = methodName;
    }
}
