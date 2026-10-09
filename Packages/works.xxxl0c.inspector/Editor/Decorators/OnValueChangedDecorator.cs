using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [OnValueChanged] の装飾。値が変わるたびに、フィールドの持ち主インスタンスの
    /// 引数なしメソッドを呼ぶ。メソッド名の妥当性は構築時ではなく <see cref="OnValueChangedRule"/>
    /// （検証層）が担当する。ここでは呼び出しだけに専念する。
    /// </summary>
    /// <remarks>
    /// 変更の検知は本体の種類に依存しないよう行コンテナで行う（IFieldFactory が差し替えた本体や、
    /// 自前展開したネストクラスでも効く）。複数選択中は選択中のオブジェクトすべてで呼ぶ。
    /// </remarks>
    public sealed class OnValueChangedDecorator : IPropertyDecorator
    {
        private const BindingFlags METHOD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public Type AttributeType => typeof(OnValueChangedAttribute);

        public int Order => 40;

        public void Decorate(DecorationContext context)
        {
            OnValueChangedAttribute attribute = (OnValueChangedAttribute)context.Attribute;
            context.Row.TrackPropertyValue(context.Property, changed => InvokeOnTargets(changed, attribute.MethodName));
        }

        private static void InvokeOnTargets(SerializedProperty changed, string methodName)
        {
            Object[] targets = changed.serializedObject.targetObjects;
            if (targets.Length == 1)
            {
                Invoke(changed, methodName, targets[0]);
                return;
            }

            // 持ち主の解決は SerializedObject の先頭のオブジェクトを辿るので、1つずつ作り直す
            foreach (Object target in targets)
            {
                using (SerializedObject single = new SerializedObject(target))
                {
                    SerializedProperty property = single.FindProperty(changed.propertyPath);
                    if (property != null) Invoke(property, methodName, target);
                }
            }
        }

        private static void Invoke(SerializedProperty property, string methodName, Object target)
        {
            object owner = SerializedFieldUtility.ResolveOwnerObject(property);
            if (owner == null) return;

            MethodInfo method = owner.GetType().GetMethod(
                methodName, METHOD_FLAGS, binder: null, types: Type.EmptyTypes, modifiers: null);

            // メソッドが見つからない場合はここでは何もしない。検証層が Warning で報告する
            if (method == null) return;

            try
            {
                method.Invoke(owner, null);
            }
            catch (TargetInvocationException exception)
            {
                // 1つが例外を投げても、残りのオブジェクトへの呼び出しは続ける
                Debug.LogException(exception.InnerException ?? exception, target);
            }
        }
    }
}
