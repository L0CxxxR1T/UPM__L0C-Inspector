using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// グループツリーの1ノード。バッジの伝播と空グループ判定の再帰をここに閉じ込める。
    /// ルートノード（インスペクタ全体、または展開したネストクラスの中身）はコンテナを持たない。
    /// </summary>
    internal sealed class GroupNode
    {
        private const string BADGE_TOOLTIP_ERROR = "配下に {0} 件のエラーがあります。";
        private const string BADGE_TOOLTIP_WARNING = "配下に {0} 件の警告があります。";

        private readonly List<GroupNode> _children = new List<GroupNode>();
        private readonly List<InspectorRow> _rows = new List<InspectorRow>();
        private readonly Label _badge;

        // ボタンなど InspectorRow を経由しない常時表示コンテンツ。空グループ判定の対象外にする
        private bool _hasStaticContent;

        /// <summary>ツリー内で一意なキー。パスの衝突判定と viewDataKey に使う。</summary>
        public string Key { get; }

        /// <summary>このノードを作ったファクトリの型。種類の食い違い検出に使う。</summary>
        public Type FactoryType { get; }

        /// <summary>ルートノードなら null。</summary>
        public GroupContainer Container { get; }

        /// <summary>ページを持つグループ（タブ群など）の入れ物なら、そのファクトリ。それ以外は null。</summary>
        public IGroupPageFactory PageFactory { get; }

        /// <summary>ページを持つグループの1ページか。</summary>
        public bool IsPage { get; }

        /// <summary>配下の行や入れ子グループを入れる要素。</summary>
        public VisualElement Content { get; }

        /// <summary>ルートノード用。</summary>
        public GroupNode(string key, VisualElement content)
        {
            Key = key;
            Content = content;
        }

        /// <summary>グループノード用。</summary>
        public GroupNode(
            string key, GroupContainer container, Type factoryType, IGroupPageFactory pageFactory, bool isPage)
        {
            Key = key;
            Container = container;
            FactoryType = factoryType;
            PageFactory = pageFactory;
            IsPage = isPage;
            Content = container.Content;

            _badge = new Label();
            _badge.AddToClassList(InspectorClassNames.BADGE);
            _badge.style.display = DisplayStyle.None;
            container.BadgeHost.Add(_badge);
        }

        public void AddChild(GroupNode child) => _children.Add(child);

        public void AddRow(InspectorRow row) => _rows.Add(row);

        /// <summary>
        /// ボタンなど、検証を持たず常に表示され続ける要素を追加したときに呼ぶ。
        /// 呼んでおかないと、行が1つも無いグループが「空グループ」として隠されてしまう。
        /// </summary>
        public void MarkHasStaticContent() => _hasStaticContent = true;

        /// <summary>
        /// 葉から根へ集計し、バッジと空グループ判定を反映する。
        /// Foldout の開閉状態は触らない（エラーが出ても自動で開かない）。
        /// </summary>
        public Result Update()
        {
            int errors = 0;
            int warnings = 0;
            int visibleRows = 0;
            bool hasStaticContent = _hasStaticContent;

            foreach (InspectorRow row in _rows)
            {
                if (!row.IsVisible) continue;

                visibleRows++;
                errors += row.ErrorCount;
                warnings += row.WarningCount;
            }

            foreach (GroupNode child in _children)
            {
                Result childResult = child.Update();
                errors += childResult.ErrorCount;
                warnings += childResult.WarningCount;
                visibleRows += childResult.VisibleRowCount;
                hasStaticContent |= childResult.HasStaticContent;
            }

            if (Container != null)
            {
                ApplyBadge(errors, warnings);

                // 子が全て非表示かつ常時表示コンテンツも無いグループだけを隠す
                Container.SetVisible(visibleRows > 0 || hasStaticContent);
            }

            return new Result(errors, warnings, visibleRows, hasStaticContent);
        }

        private void ApplyBadge(int errors, int warnings)
        {
            _badge.EnableInClassList(InspectorClassNames.BADGE_ERROR, errors > 0);
            _badge.EnableInClassList(InspectorClassNames.BADGE_WARNING, errors == 0 && warnings > 0);

            if (errors > 0)
            {
                _badge.text = errors.ToString();
                _badge.tooltip = string.Format(BADGE_TOOLTIP_ERROR, errors);
                _badge.style.display = DisplayStyle.Flex;
                return;
            }

            if (warnings > 0)
            {
                _badge.text = warnings.ToString();
                _badge.tooltip = string.Format(BADGE_TOOLTIP_WARNING, warnings);
                _badge.style.display = DisplayStyle.Flex;
                return;
            }

            _badge.style.display = DisplayStyle.None;
        }

        internal readonly struct Result
        {
            public int ErrorCount { get; }
            public int WarningCount { get; }
            public int VisibleRowCount { get; }
            public bool HasStaticContent { get; }

            public Result(int errorCount, int warningCount, int visibleRowCount, bool hasStaticContent)
            {
                ErrorCount = errorCount;
                WarningCount = warningCount;
                VisibleRowCount = visibleRowCount;
                HasStaticContent = hasStaticContent;
            }
        }
    }
}
