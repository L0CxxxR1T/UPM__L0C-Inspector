namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// インスペクタの自前展開と検証の再帰で共有する定数。
    /// ここがズレると、インスペクタで展開される深さと検証が「展開されている」とみなす深さが食い違い、
    /// 3層（インスペクタ / 一括チェック / ビルド前フック）の結果が一致しなくなる。
    /// </summary>
    internal static class InspectionLimits
    {
        /// <summary>ネストしたクラスを自前展開してよい深さの上限。Unity のシリアライズ深度上限に合わせる。</summary>
        public const int MAX_NESTED_DEPTH = 7;
    }
}
