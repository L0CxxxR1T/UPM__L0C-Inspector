namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 一括チェック・ビルド前フックが報告する1件の検証結果。
    /// シーンを閉じると Object 参照は死ぬので、対象は GlobalObjectId の文字列で持つ。
    /// </summary>
    public readonly struct ValidationIssue
    {
        /// <summary>アセットのパス。シーン内オブジェクトなら、そのシーンのパス。</summary>
        public string AssetPath { get; }

        /// <summary>ヒエラルキー上のパス（表示用）。アセット単体なら空文字。</summary>
        public string ObjectPath { get; }

        public string ComponentTypeName { get; }
        public string PropertyPath { get; }

        /// <summary>対象を後から引き直すためのキー。GlobalObjectId.ToString()。</summary>
        public string GlobalObjectId { get; }

        public ValidationMessage Message { get; }

        public ValidationIssue(
            string assetPath,
            string objectPath,
            string componentTypeName,
            string propertyPath,
            string globalObjectId,
            ValidationMessage message)
        {
            AssetPath = assetPath;
            ObjectPath = objectPath;
            ComponentTypeName = componentTypeName;
            PropertyPath = propertyPath;
            GlobalObjectId = globalObjectId;
            Message = message;
        }
    }
}
