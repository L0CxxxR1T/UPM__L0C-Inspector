namespace XXXL0C.Inspector.Editor
{
    /// <summary>USS のクラス名。Styles/InspectorStyles.uss と対応させる。</summary>
    internal static class InspectorClassNames
    {
        public const string ROOT = "l0c-inspector";
        public const string ROW = "l0c-inspector__row";
        public const string MESSAGES = "l0c-inspector__messages";
        public const string MESSAGE = "l0c-inspector__message";

        public const string NESTED = "l0c-inspector__nested";

        public const string GROUP = "l0c-inspector__group";
        public const string GROUP_BOX = "l0c-inspector__group--box";
        public const string GROUP_FOLDOUT = "l0c-inspector__group--foldout";
        public const string GROUP_HEADER = "l0c-inspector__group-header";
        public const string GROUP_CONTENT = "l0c-inspector__group-content";

        public const string BADGE = "l0c-inspector__badge";
        public const string BADGE_ERROR = "l0c-inspector__badge--error";
        public const string BADGE_WARNING = "l0c-inspector__badge--warning";

        /// <summary>IFieldFactory が辞退したときの理由表示。検証メッセージ（MESSAGES）とは別の永続領域。</summary>
        public const string NOTICE = "l0c-inspector__notice";

        public const string AFFIX_ROW = "l0c-inspector__affix-row";
        public const string AFFIX_LABEL = "l0c-inspector__affix-label";

        public const string INLINE_EDITOR = "l0c-inspector__inline-editor";

        public const string BUTTON = "l0c-inspector__button";
        public const string BUTTON_FOOTER = "l0c-inspector__button-footer";
    }
}
