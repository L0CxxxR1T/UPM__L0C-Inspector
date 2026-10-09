using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>デコレータに渡す構築時の情報。</summary>
    public sealed class DecorationContext
    {
        public SerializedProperty Property { get; }

        /// <summary>
        /// 加工対象のフィールド本体。通常は <see cref="PropertyField"/>、
        /// 自前展開したネストクラスでは <see cref="Foldout"/>、Dictionary では追加欄を含むコンテナ、
        /// IFieldFactory が本体を差し替えた場合はその要素になる。
        /// </summary>
        public VisualElement Field { get; }

        /// <summary>
        /// 本体の PropertyField。Dictionary ではコンテナの中のものを返す。
        /// 展開されたネストクラスと、IFieldFactory が差し替えた本体では null。
        /// </summary>
        public PropertyField AsPropertyField
            => Field is DictionaryField dictionaryField ? dictionaryField.PropertyField : Field as PropertyField;

        /// <summary>IFieldFactory が差し替えた本体の表示名ラベル。それ以外では null。</summary>
        public Label FactoryLabel { get; }

        /// <summary>Field と検証メッセージ領域を含む行コンテナ。要素を差し込みたいときに使う。</summary>
        public VisualElement Row { get; }

        public FieldInfo FieldInfo { get; }

        /// <summary>この呼び出しの契機になった属性インスタンス。</summary>
        public Attribute Attribute { get; }

        public DecorationContext(
            SerializedProperty property,
            VisualElement field,
            Label factoryLabel,
            VisualElement row,
            FieldInfo fieldInfo,
            Attribute attribute)
        {
            Property = property;
            Field = field;
            FactoryLabel = factoryLabel;
            Row = row;
            FieldInfo = fieldInfo;
            Attribute = attribute;
        }
    }
}
