using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// フィールドの値を、static readonly フィールドに並べた候補から選ばせる。
    /// 候補は値そのものを並べた配列・List か、表示名を付けたい場合は <see cref="ValueDropdownList{T}"/> を使う。
    /// 保存されている値が候補に無ければ検証層が Warning を出す。
    /// </summary>
    /// <remarks>
    /// 候補の取得元を static readonly フィールドに限るのは、検証のときにユーザーコード（メソッドやプロパティ）を
    /// 実行しないため。配列・List のフィールドには未対応。
    /// </remarks>
    /// <example>
    /// <code>
    /// private static readonly ValueDropdownList&lt;int&gt; SPEEDS = new ValueDropdownList&lt;int&gt;
    /// {
    ///     { "遅い", 1 },
    ///     { "速い", 5 }
    /// };
    ///
    /// [ValueDropdown(nameof(SPEEDS))]
    /// [SerializeField] private int _speed = 1;
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ValueDropdownAttribute : Attribute
    {
        /// <summary>候補を持つフィールドを宣言している型。null ならこのフィールドを宣言している型。</summary>
        public Type SourceType { get; }

        /// <summary>候補を持つ static readonly フィールドの名前。nameof で指定する。</summary>
        public string FieldName { get; }

        public ValueDropdownAttribute(string fieldName) : this(null, fieldName)
        {
        }

        public ValueDropdownAttribute(Type sourceType, string fieldName)
        {
            SourceType = sourceType;
            FieldName = fieldName;
        }
    }
}
