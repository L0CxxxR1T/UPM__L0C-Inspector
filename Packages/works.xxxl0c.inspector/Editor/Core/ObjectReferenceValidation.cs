using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [AssetsOnly] / [SceneObjectsOnly] / [ChildGameObjectsOnly] が共有する、参照先の取り出しと判定。
    /// </summary>
    internal static class ObjectReferenceValidation
    {
        /// <summary>
        /// 検査する参照先を返す。未設定・検査対象外なら null。
        /// Object 参照を持てない型に付いていれば、ここで Warning を出す。
        /// </summary>
        public static Object GetTarget(ValidationContext context, string attributeName)
        {
            SerializedProperty property = context.Property;

            switch (context.Role)
            {
                case CollectionRole.Collection:
                    // 要素側で判定する
                    return null;

                case CollectionRole.Dictionary:
                    DictionaryUtility.TryGetKeyValueTypes(context.ValueType, out Type keyType, out Type valueType);
                    if (!IsObjectType(keyType) && !IsObjectType(valueType))
                    {
                        context.Report(
                            ValidationSeverity.Warning,
                            $"[{attributeName}] はキーにも値にも Object 参照を持たない Dictionary には効果がありません。");
                    }
                    return null;

                case CollectionRole.DictionaryKey:
                case CollectionRole.DictionaryValue:
                    // 効果の有無はコンテナで1回だけ判定したので、Object 参照でない側は黙って飛ばす
                    return property.propertyType == SerializedPropertyType.ObjectReference
                        ? property.objectReferenceValue
                        : null;

                default:
                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        context.Report(
                            ValidationSeverity.Warning,
                            $"[{attributeName}] は {property.propertyType} 型には効果がありません。Object 参照にのみ使えます。");
                        return null;
                    }

                    return property.objectReferenceValue;
            }
        }

        /// <summary>
        /// 実行時にシーン上のオブジェクトとして扱われる参照か。
        /// プレハブアセットの中で同じプレハブの GameObject / Component を指す参照は、
        /// インスタンス化するとシーン上のオブジェクトになるのでこちらに含める。
        /// </summary>
        public static bool IsSceneObject(Object value, Object owner)
        {
            if (!EditorUtility.IsPersistent(value)) return true;
            if (!(value is GameObject) && !(value is Component)) return false;
            if (owner == null || !EditorUtility.IsPersistent(owner)) return false;

            return string.Equals(
                AssetDatabase.GetAssetPath(value), AssetDatabase.GetAssetPath(owner), StringComparison.Ordinal);
        }

        private static bool IsObjectType(Type type) => type != null && typeof(Object).IsAssignableFrom(type);
    }
}
