namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 検証対象のプロパティが、属性の付いたフィールドから見てどの位置にあるか。
    /// コレクションの要素は親フィールドの属性を引き継いで検証されるので、ルール側が区別するのに使う。
    /// </summary>
    public enum CollectionRole
    {
        /// <summary>フィールドそのもの。</summary>
        None,

        /// <summary>配列 / List の要素。</summary>
        Element,

        /// <summary>Dictionary そのもの（要素ではなくコンテナ）。</summary>
        Dictionary,

        /// <summary>Dictionary の要素のキー。</summary>
        DictionaryKey,

        /// <summary>Dictionary の要素の値。</summary>
        DictionaryValue
    }
}
