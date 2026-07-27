using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 属性に応じてフィールドの表示 / 非表示を決める拡張点。
    /// 装飾（構築時1回）でも検証（メッセージ）でもない第3の軸で、更新パスの検証より前に走る。
    /// TypeCache で自動収集されるので、実装を足すだけで有効になる（引数なしコンストラクタが必要）。
    /// </summary>
    public interface IVisibilityRule
    {
        /// <summary>担当する属性の型。</summary>
        Type AttributeType { get; }

        /// <summary>false を返したルールが1つでもあれば非表示になる（AND）。</summary>
        bool IsVisible(VisibilityContext context);
    }
}
