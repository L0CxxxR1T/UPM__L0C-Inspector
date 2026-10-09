namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// ページを持つグループ（タブなど）の拡張点。<see cref="IGroupContainerFactory.Create"/> でページの入れ物を作り、
    /// <see cref="CreatePage"/> でページを足す。
    /// </summary>
    /// <remarks>
    /// パスは「…/入れ物/ページ」の形で、末尾がページ、その1つ手前が入れ物になる。
    /// 入れ物の直下に作られるグループは、どのグループ属性から作られてもページになる。
    /// </remarks>
    public interface IGroupPageFactory : IGroupContainerFactory
    {
        /// <param name="owner">Create が返した入れ物。</param>
        /// <param name="displayName">ページの見出しに出す名前。</param>
        /// <param name="viewDataKey">開閉状態などを保持するための一意なキー。</param>
        /// <returns>owner に追加済みのページ。</returns>
        GroupContainer CreatePage(GroupContainer owner, string displayName, string viewDataKey);
    }
}
