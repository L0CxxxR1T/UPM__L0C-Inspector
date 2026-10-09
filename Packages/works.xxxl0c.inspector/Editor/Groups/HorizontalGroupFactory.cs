using System;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [HorizontalGroup] を横並びのコンテナとして組み立てる。各列は等幅にする。
    /// </summary>
    /// <remarks>
    /// インスペクタのフィールドはラベル幅をインスペクタ全体の幅から決める（aligned）ので、
    /// 列の中ではラベルが場所を取りすぎる。列の中のフィールドは揃えを外し、ラベルを中身の幅に戻す。
    /// </remarks>
    public sealed class HorizontalGroupFactory : IGroupContainerFactory
    {
        private static readonly string ALIGNED_CLASS = BaseField<int>.alignedFieldUssClassName;
        private static readonly string LABEL_CLASS = BaseField<int>.labelUssClassName;

        public Type AttributeType => typeof(HorizontalGroupAttribute);

        public string GetPath(Attribute attribute) => ((HorizontalGroupAttribute)attribute).Path;

        public GroupContainer Create(string displayName, string viewDataKey)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList(InspectorClassNames.GROUP);
            root.AddToClassList(InspectorClassNames.GROUP_HORIZONTAL);

            // PropertyField は中身のフィールドをバインド時に作るので、作られた後に外す
            root.RegisterCallback<GeometryChangedEvent>(_ => ReleaseAlignment(root));

            // 見出しを持たないのでバッジは出さない。配下のメッセージは各列の下に出る
            return new GroupContainer(root, root, new VisualElement());
        }

        private static void ReleaseAlignment(VisualElement root)
        {
            root.Query(className: ALIGNED_CLASS).ForEach(field =>
            {
                field.RemoveFromClassList(ALIGNED_CLASS);

                // 揃えていたときの幅はインラインで書かれているので、USS に戻すには消す必要がある
                Label label = field.Q<Label>(className: LABEL_CLASS);
                if (label == null) return;

                label.style.width = StyleKeyword.Null;
                label.style.minWidth = StyleKeyword.Null;
            });
        }
    }
}
