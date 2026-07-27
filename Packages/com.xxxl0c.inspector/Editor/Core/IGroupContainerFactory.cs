using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// グループ属性からコンテナを組み立てる拡張点。グループの種類を足したいときはこれを実装する。
    /// TypeCache で自動収集されるので、実装を足すだけで有効になる（引数なしコンストラクタが必要）。
    /// </summary>
    public interface IGroupContainerFactory
    {
        /// <summary>担当する属性の型。</summary>
        Type AttributeType { get; }

        /// <summary>属性からグループのパスを取り出す。"/" 区切りで入れ子になる。</summary>
        string GetPath(Attribute attribute);

        /// <param name="displayName">パスの末尾セグメント。ヘッダに出す名前。</param>
        /// <param name="viewDataKey">開閉状態などを保持するための一意なキー。</param>
        GroupContainer Create(string displayName, string viewDataKey);
    }
}
