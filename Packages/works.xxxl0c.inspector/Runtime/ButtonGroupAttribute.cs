using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// 同じ名前のボタンを横1列に並べる。[Button] が無くても、付けたメソッドは既定の設定のボタンになる。
    /// 列は、その名前を持つ最初のボタンの位置（[Button] の group で指定したグループの中）に出る。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ButtonGroupAttribute : Attribute
    {
        public string Name { get; }

        public ButtonGroupAttribute(string name) => Name = name;
    }
}
