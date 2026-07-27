using System;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[BoxGroup] を枠付きコンテナとして組み立てる。</summary>
    public sealed class BoxGroupFactory : IGroupContainerFactory
    {
        public Type AttributeType => typeof(BoxGroupAttribute);

        public string GetPath(Attribute attribute) => ((BoxGroupAttribute)attribute).Path;

        public GroupContainer Create(string displayName, string viewDataKey)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList(InspectorClassNames.GROUP);
            root.AddToClassList(InspectorClassNames.GROUP_BOX);

            VisualElement header = new VisualElement();
            header.AddToClassList(InspectorClassNames.GROUP_HEADER);
            header.Add(new Label(displayName));
            root.Add(header);

            VisualElement content = new VisualElement();
            content.AddToClassList(InspectorClassNames.GROUP_CONTENT);
            root.Add(content);

            return new GroupContainer(root, content, header);
        }
    }
}
