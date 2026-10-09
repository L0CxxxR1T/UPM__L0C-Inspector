namespace XXXL0C.Inspector
{
    /// <summary>[Button] がどのモードで実行可能かを制限する。</summary>
    public enum ButtonMode
    {
        /// <summary>EditMode / PlayMode どちらでも実行できる。</summary>
        Always,

        /// <summary>EditMode 中だけ実行できる。PlayMode 中は無効化される。</summary>
        EditorOnly,

        /// <summary>PlayMode 中だけ実行できる。EditMode 中は無効化される。</summary>
        PlayModeOnly
    }
}
