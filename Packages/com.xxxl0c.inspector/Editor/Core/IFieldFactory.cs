using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 属性に応じてフィールドの本体要素を作る拡張点。装飾（IPropertyDecorator）が
    /// 「できた本体を加工する」のに対し、こちらは「本体そのものを作る」。
    /// 1フィールドに適用できるのは1つだけ（装飾のように何個も重ねられない）。
    /// TypeCache で自動収集されるので、実装を足すだけで有効になる（引数なしコンストラクタが必要）。
    /// </summary>
    public interface IFieldFactory
    {
        /// <summary>担当する属性の型。</summary>
        Type AttributeType { get; }

        FieldFactoryResult Create(FieldFactoryContext context);
    }
}
