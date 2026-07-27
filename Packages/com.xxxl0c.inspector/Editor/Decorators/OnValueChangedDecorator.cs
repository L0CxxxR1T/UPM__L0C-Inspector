using System;
using System.Reflection;
using UnityEditor.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [OnValueChanged] の装飾。値が変わるたびに、フィールドの持ち主インスタンスの
    /// 引数なしメソッドを呼ぶ。メソッド名の妥当性は構築時ではなく <see cref="OnValueChangedRule"/>
    /// （検証層）が担当する。ここでは呼び出しだけに専念する。
    /// </summary>
    public sealed class OnValueChangedDecorator : IPropertyDecorator
    {
        public Type AttributeType => typeof(OnValueChangedAttribute);

        public int Order => 40;

        public void Decorate(DecorationContext context)
        {
            PropertyField field = context.AsPropertyField;
            if (field == null) return; // 自前展開されたネストクラスの Foldout 自体には適用しない

            OnValueChangedAttribute attribute = (OnValueChangedAttribute)context.Attribute;

            field.RegisterValueChangeCallback(changeEvent =>
            {
                object owner = SerializedFieldUtility.ResolveOwnerObject(changeEvent.changedProperty);
                if (owner == null) return;

                MethodInfo method = owner.GetType().GetMethod(
                    attribute.MethodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null, types: Type.EmptyTypes, modifiers: null);

                // メソッドが見つからない場合はここでは何もしない。検証層が Warning で報告する
                method?.Invoke(owner, null);
            });
        }
    }
}
