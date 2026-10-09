namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 装飾が反映されなくても、属性の本来の役割は検証層で果たせる装飾の印。
    /// 自前展開されないネスト（List の要素の中など）にあっても「反映されません」の警告を出さない。
    /// </summary>
    /// <remarks>
    /// 例: [MinValue] の丸めはインスペクタの補助で、範囲外の検出は検証ルールが3層とも担う。
    /// 警告を出すと、効いている属性を「効いていない」と誤解させてしまう。
    /// </remarks>
    internal interface ISupplementaryDecorator
    {
    }
}
