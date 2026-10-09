using System;
using System.Reflection;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [OnValueChanged] のメソッド名を検証する。タイポで存在しないメソッドを指定していても
    /// OnValueChangedDecorator は静かに何もしないだけなので、ここで気づけるようにする。
    /// 検証層に置くことで一括チェックウィンドウからもプロジェクト横断で拾える。
    /// </summary>
    public sealed class OnValueChangedRule : IValidationRule
    {
        private const BindingFlags METHOD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public Type AttributeType => typeof(OnValueChangedAttribute);

        public void Validate(ValidationContext context)
        {
            // 要素は配列フィールドの属性を引き継いでいるだけ。持ち主をたどるとコレクション自体になってしまう
            if (context.Role != CollectionRole.None
                && context.Role != CollectionRole.Collection
                && context.Role != CollectionRole.Dictionary)
            {
                return;
            }

            OnValueChangedAttribute attribute = (OnValueChangedAttribute)context.Attribute;

            object owner = SerializedFieldUtility.ResolveOwnerObject(context.Property);
            if (owner == null) return; // 持ち主を辿れない状態は他の層の問題なので、ここでは何も言わない

            MethodInfo method = owner.GetType().GetMethod(
                attribute.MethodName, METHOD_FLAGS, binder: null, types: Type.EmptyTypes, modifiers: null);

            if (method == null)
            {
                context.Report(
                    ValidationSeverity.Warning,
                    $"[OnValueChanged] のメソッド '{attribute.MethodName}' が見つかりません。"
                    + "nameof で指定しているか確認してください。");
            }
        }
    }
}
