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

        private readonly Dictionary<string, GroupNode> _nodes = new Dictionary<string, GroupNode>(StringComparer.Ordinal);

        /// <summary>
        /// ルートノードを作る。インスペクタ全体と、展開したネストクラスの中身がそれぞれルートになる。
        /// </summary>
        public GroupNode CreateRoot(string key, VisualElement content) => new GroupNode(key, content);

        /// <summary>
        /// パスをたどってノードを返す。パスが空なら親ノードをそのまま返す。
        /// </summary>
        public GroupNode Resolve(GroupNode parent, string path, IGroupContainerFactory factory)
        {
            if (string.IsNullOrEmpty(path)) return parent;

            GroupNode current = parent;
            foreach (string rawSegment in path.Split(SEPARATOR))
            {
                string segment = rawSegment.Trim();
                if (segment.Length == 0) continue;

                current = ResolveSegment(current, segment, factory);
            }

            return current;
        }

        private GroupNode ResolveSegment(GroupNode parent, string segment, IGroupContainerFactory factory)
        {
            string key = $"{parent.Key}{SEPARATOR}{segment}";

            if (_nodes.TryGetValue(key, out GroupNode existing))
            {
                if (existing.FactoryType != factory.GetType())
                {
                    Debug.LogWarning(
                        $"{LOG_PREFIX}グループ '{key}' に種類の違うグループ属性が付いています。"
                        + $"先に作られた {existing.FactoryType.Name} を使用します。");
                }

                return existing;
            }

            GroupContainer container = factory.Create(segment, key);
            GroupNode node = new GroupNode(key, container, factory.GetType());

            parent.AddChild(node);
            parent.Content.Add(container.Root);
            _nodes.Add(key, node);

            return node;
        }
    }
}
