using System;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[TabGroup] をタブ群（TabView）とページ（Tab）として組み立てる。</summary>
    public sealed class TabGroupFactory : IGroupPageFactory
    {
        public Type AttributeType => typeof(TabGroupAttribute);

        public string GetPath(Attribute attribute)
        {
            TabGroupAttribute tabGroup = (TabGroupAttribute)attribute;
            return $"{tabGroup.Group}/{tabGroup.Tab}";
        }

        public GroupContainer Create(string displayName, string viewDataKey)
        {
            TabView tabView = new TabView { viewDataKey = viewDataKey };
            tabView.AddToClassList(InspectorClassNames.GROUP);
            tabView.AddToClassList(InspectorClassNames.GROUP_TAB);

            // タブ群自体は見出しを持たないのでバッジは出さない。件数は各ページの見出しに出る
            return new GroupContainer(tabView, tabView, new VisualElement());
        }

        public GroupContainer CreatePage(GroupContainer owner, string displayName, string viewDataKey)
        {
            Tab tab = new Tab(displayName) { viewDataKey = viewDataKey };
            tab.AddToClassList(InspectorClassNames.GROUP_TAB_PAGE);
            owner.Root.Add(tab);

            // ページを隠すときは見出しも消す。見出しは TabView 側に移されるので Root には含まれない
            return new GroupContainer(tab, tab, tab.tabHeader, tab.tabHeader);
        }
    }
}
