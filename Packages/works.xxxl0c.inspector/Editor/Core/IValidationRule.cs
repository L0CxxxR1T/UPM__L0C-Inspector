using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 属性に応じて値を検証する拡張点。装飾（IPropertyDecorator）とは別レイヤーで、
    /// ツリー構築後の更新パスから何度も呼ばれる。
    /// TypeCache で自動収集されるので、実装を足すだけで有効になる（引数なしコンストラクタが必要）。
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>担当する属性の型。</summary>
        Type AttributeType { get; }

        void Validate(ValidationContext context);
    }
}
