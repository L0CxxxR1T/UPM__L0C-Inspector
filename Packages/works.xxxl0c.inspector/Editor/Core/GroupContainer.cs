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

        /// <summary>
        /// Root の外にある見出し（タブの見出しなど）。グループが空になったら Root と一緒に隠す。無ければ null。
        /// </summary>
        public VisualElement DetachedHeader { get; }

        public GroupContainer(VisualElement root, VisualElement content, VisualElement badgeHost)
            : this(root, content, badgeHost, null)
        {
        }

        public GroupContainer(VisualElement root, VisualElement content, VisualElement badgeHost, VisualElement detachedHeader)
        {
            Root = root;
            Content = content;
            BadgeHost = badgeHost;
            DetachedHeader = detachedHeader;
        }

        internal void SetVisible(bool visible)
        {
            DisplayStyle display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            Root.style.display = display;
            if (DetachedHeader != null) DetachedHeader.style.display = display;
        }
    }
}
