using System;
using System.Reflection;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>フィールドファクトリに渡す構築時の情報。</summary>
    public sealed class FieldFactoryContext
    {
        public SerializedProperty Property { get; }
        public FieldInfo FieldInfo { get; }

        /// <summary>この呼び出しの契機になった属性インスタンス。</summary>
        public Attribute Attribute { get; }

        public FieldFactoryContext(SerializedProperty property, FieldInfo fieldInfo, Attribute attribute)
        {
            Property = property;
            FieldInfo = fieldInfo;
            Attribute = attribute;
        }
    }
}
