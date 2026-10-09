using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 1オブジェクトを検証する。UI に依存しないので、インスペクタ・一括チェック・ビルド前フックの
    /// どこから呼んでも同じ結果になる。
    /// </summary>
    public static class ObjectValidator
    {
        private static readonly List<ValidationMessage> _buffer = new List<ValidationMessage>();

        /// <summary>
        /// target を検証し、結果を issues に追加する。
        /// </summary>
        /// <param name="assetPath">結果に添える参照元パス（シーン内オブジェクトならそのシーンのパス）。</param>
        /// <param name="objectPath">ヒエラルキー上のパス。アセット単体なら空文字。</param>
        public static void Validate(Object target, string assetPath, string objectPath, List<ValidationIssue> issues)
        {
            if (target == null) return;

            // プロジェクト全体を走査するので、このゲートが性能の要。
            // 扱う属性を1つも持たないオブジェクトは SerializedObject すら作らない
            if (!InspectedTypeCache.ContainsHandledAttributes(target.GetType())) return;

            SerializedObject serializedObject = new SerializedObject(target);
            string globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
            string componentTypeName = target.GetType().Name;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                FieldInfo fieldInfo = SerializedFieldUtility.FindSerializedField(target.GetType(), iterator.name);
                if (fieldInfo == null) continue;

                SerializedProperty property = iterator.Copy();
                _buffer.Clear();

                // インスペクタと同じ判定を通す。ここがズレると3層の結果が食い違う
                bool visible = VisibilityEvaluator.IsVisible(property, fieldInfo, property.displayName, _buffer);

                // 非表示のフィールドは検証しない（設定できないものを未設定だと責めない）。
                // ただし可視性ルール自体が誤用で Warning を出していた場合は、それだけは報告する
                if (visible)
                {
                    ValidationWalker.Collect(property, fieldInfo, _buffer, hasOwnRow: null);
                }

                foreach (ValidationMessage message in _buffer)
                {
                    issues.Add(new ValidationIssue(
                        assetPath, objectPath, componentTypeName, property.propertyPath, globalObjectId, message));
                }
            }
        }
    }
}
