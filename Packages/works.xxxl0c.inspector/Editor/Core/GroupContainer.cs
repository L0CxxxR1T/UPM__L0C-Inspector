using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>グループコンテナの構成要素。ファクトリが組み立てて返す。</summary>
    public sealed class GroupContainer
    {
        /// <summary>親に足す最上位の要素。</summary>
        public VisualElement Root { get; }

        /// <summary>配下の行や入れ子グループを入れる要素。</summary>
        public VisualElement Content { get; }

        /// <summary>
        /// 検証バッジを置く要素。折りたたんでもバッジが見えるよう、ヘッダ側を指定すること。
        /// </summary>
        public VisualElement BadgeHost { get; }

        public GroupContainer(VisualElement root, VisualElement content, VisualElement badgeHost)
        {
            Root = root;
            Content = content;
            BadgeHost = badgeHost;
        }
    }
}
