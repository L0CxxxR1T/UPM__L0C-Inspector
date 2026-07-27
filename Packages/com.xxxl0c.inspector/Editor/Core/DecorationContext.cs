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
        /// 自前展開したネストクラスでは <see cref="Foldout"/> になる。
        /// </summary>
        public VisualElement Field { get; }

        /// <summary>Field が PropertyField のときだけ取れる。展開されたネストクラスでは null。</summary>
        public PropertyField AsPropertyField => Field as PropertyField;

        /// <summary>Field と検証メッセージ領域を含む行コンテナ。要素を差し込みたいときに使う。</summary>
        public VisualElement Row { get; }

        public FieldInfo FieldInfo { get; }

        /// <summary>この呼び出しの契機になった属性インスタンス。</summary>
        public Attribute Attribute { get; }

        public DecorationContext(
            SerializedProperty property,
            VisualElement field,
            VisualElement row,
            FieldInfo fieldInfo,
            Attribute attribute)
        {
            Property = property;
            Field = field;
            Row = row;
            FieldInfo = fieldInfo;
            Attribute = attribute;
        }
    }
}
