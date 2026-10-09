using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// グループパスからノードを引く / 作る。
    /// グループは「そのグループに属する最初のフィールドが現れた位置」に出るので、
    /// 未作成のセグメントを見つけたその場で親の Content に足す。順序引数は持たない。
    /// </summary>
    internal sealed class GroupTree
    {
        private const char SEPARATOR = '/';
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";

        // ページを持つグループ（タブ）のパスで、入れ物より外側のセグメントが未作成だったときに使う
        private static readonly FoldoutGroupFactory _outerFactory = new FoldoutGroupFactory();

        private readonly Dictionary<string, GroupNode> _nodes = new Dictionary<string, GroupNode>(StringComparer.Ordinal);

        /// <summary>
        /// ルートノードを作る。インスペクタ全体と、展開したネストクラスの中身がそれぞれルートになる。
        /// </summary>
        public GroupNode CreateRoot(string key, VisualElement content) => new GroupNode(key, content);

        /// <summary>
        /// パスをたどってノードを返す。パスが空なら親ノードをそのまま返す。
        /// </summary>
        public GroupNode Resolve(
            GroupNode parent, string path, IGroupContainerFactory factory, bool allowExistingTypeMismatch = false)
        {
            List<string> segments = SplitPath(path);
            if (segments.Count == 0) return parent;

            IGroupPageFactory pageFactory = factory as IGroupPageFactory;
            if (pageFactory != null && segments.Count < 2)
            {
                Debug.LogWarning($"{LOG_PREFIX}グループ '{path}' には、入れ物とページの両方の名前が必要です。");
                return parent;
            }

            GroupNode current = parent;
            for (int i = 0; i < segments.Count; i++)
            {
                // ページを持つグループは「…/入れ物/ページ」。入れ物より外側は普通のグループとして扱う
                IGroupContainerFactory segmentFactory =
                    pageFactory != null && i < segments.Count - 2 ? _outerFactory : factory;

                current = ResolveSegment(
                    current, segments[i], segmentFactory, i == segments.Count - 1, allowExistingTypeMismatch);
            }

            if (current.PageFactory != null)
            {
                // ページの入れ物（タブ群）には行を直接置けない
                Debug.LogWarning(
                    $"{LOG_PREFIX}グループ '{current.Key}' はページの入れ物なので、直接は入れられません。"
                    + "ページ名まで指定してください。");
                return parent;
            }

            return current;
        }

        private GroupNode ResolveSegment(
            GroupNode parent, string segment, IGroupContainerFactory factory, bool isLast, bool allowTypeMismatch)
        {
            string key = $"{parent.Key}{SEPARATOR}{segment}";

            if (_nodes.TryGetValue(key, out GroupNode existing))
            {
                // 途中のセグメントは入れ物として使うだけ、ページはどの種類のグループからも指せるので、種類は問わない
                if (!allowTypeMismatch && isLast && !existing.IsPage && existing.FactoryType != factory.GetType())
                {
                    Debug.LogWarning(
                        $"{LOG_PREFIX}グループ '{key}' に種類の違うグループ属性が付いています。"
                        + $"先に作られた {existing.FactoryType.Name} を使用します。");
                }

                return existing;
            }

            GroupNode node;
            if (parent.PageFactory != null)
            {
                // 入れ物の直下は、指定したグループの種類に関わらずページにする。ページは CreatePage が入れ物に足す
                GroupContainer page = parent.PageFactory.CreatePage(parent.Container, segment, key);
                node = new GroupNode(key, page, parent.PageFactory.GetType(), null, isPage: true);
            }
            else
            {
                GroupContainer container = factory.Create(segment, key);
                node = new GroupNode(key, container, factory.GetType(), factory as IGroupPageFactory, isPage: false);
                parent.Content.Add(container.Root);
            }

            parent.AddChild(node);
            _nodes.Add(key, node);

            return node;
        }

        private static List<string> SplitPath(string path)
        {
            List<string> segments = new List<string>();
            if (string.IsNullOrEmpty(path)) return segments;

            foreach (string rawSegment in path.Split(SEPARATOR))
            {
                string segment = rawSegment.Trim();
                if (segment.Length > 0) segments.Add(segment);
            }

            return segments;
        }
    }
}
