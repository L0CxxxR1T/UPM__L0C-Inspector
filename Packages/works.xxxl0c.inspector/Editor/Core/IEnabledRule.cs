using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 属性に応じてフィールドの編集可否を決める拡張点。可視性（IVisibilityRule）の次、検証より前に走る。
    /// 編集できないフィールドは検証しない（設定できないものを未設定だと責めない）。
    /// TypeCache で自動収集されるので、実装を足すだけで有効になる（引数なしコンストラクタが必要）。
    /// </summary>
    public interface IEnabledRule
    {
        /// <summary>担当する属性の型。</summary>
        Type AttributeType { get; }

        /// <summary>false を返したルールが1つでもあれば編集できなくなる（AND）。</summary>
        bool IsEnabled(VisibilityContext context);
    }
}
