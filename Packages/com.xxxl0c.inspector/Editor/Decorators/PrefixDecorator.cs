using System;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [Prefix] の装飾。本体の前にテキストを添える。
    /// [Suffix] と同時に付けても両方効く（旧 PropertyDrawer 方式では 1 フィールド 1 Drawer の制約で
    /// 片方しか適用されなかった）。
    /// </summary>
    public sealed class PrefixDecorator : IPropertyDecorator
    {
        public Type AttributeType => typeof(PrefixAttribute);

        public int Order => 10;

        public void Decorate(DecorationContext context)
        {
            PrefixAttribute attribute = (PrefixAttribute)context.Attribute;
            VisualElement affixRow = AffixRowUtility.GetOrCreateRow(context);
            affixRow.Insert(0, AffixRowUtility.CreateAffixLabel(attribute.Text));
        }
    }
}
