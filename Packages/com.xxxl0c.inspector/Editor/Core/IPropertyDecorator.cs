using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 属性に応じて単一フィールドの見た目を加工する拡張点。
    /// TypeCache で自動収集されるので、実装を足すだけで有効になる（引数なしコンストラクタが必要）。
    /// </summary>
    public interface IPropertyDecorator
    {
        /// <summary>担当する属性の型。</summary>
        Type AttributeType { get; }

        /// <summary>同じフィールドに複数の装飾が付いたときの適用順。小さいほど先に適用される。</summary>
        int Order { get; }

        void Decorate(DecorationContext context);
    }
}
