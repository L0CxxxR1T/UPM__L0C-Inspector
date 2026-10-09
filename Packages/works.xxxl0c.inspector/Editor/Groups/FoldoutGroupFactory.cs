using System;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[FoldoutGroup] を折りたためるコンテナとして組み立てる。</summary>
    public sealed class FoldoutGroupFactory : IGroupContainerFactory
    {
        public Type AttributeType => typeof(FoldoutGroupAttribute);

        public string GetPath(Attribute attribute) => ((FoldoutGroupAttribute)attribute).Path;

        public GroupContainer Create(string displayName, string viewDataKey)
        {
            Foldout foldout = new Foldout
            {
                text = displayName,
                value = true,
                viewDataKey = viewDataKey
            };
            foldout.AddToClassList(InspectorClassNames.GROUP);
            foldout.AddToClassList(InspectorClassNames.GROUP_FOLDOUT);

            // 折りたたんでもバッジが見えるようトグル行に置く
            Toggle toggle = foldout.Q<Toggle>();
            VisualElement badgeHost = toggle == null ? foldout : (VisualElement)toggle;

            return new GroupContainer(foldout, foldout.contentContainer, badgeHost);
        }
    }
}
