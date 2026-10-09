using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
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
                bool active = VisibilityEvaluator.IsVisible(property, fieldInfo, property.displayName, _buffer)
                    && VisibilityEvaluator.IsEnabled(property, fieldInfo, property.displayName, _buffer);

                // 非表示・編集不可のフィールドは検証しない（設定できないものを未設定だと責めない）。
                // ただし可視性・編集可否のルール自体が誤用で Warning を出していた場合は、それだけは報告する
                if (active)
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

        /// <summary>
        /// プレハブインスタンスの一部であるコンポーネントを検証し、元のプレハブと同じ結果は報告しない。
        /// ネストしたプレハブ・Variant・シーン上のインスタンスで同じ問題が二重に並ぶのを防ぐ。
        /// 元のプレハブ自体も同じ走査で検証される前提で、オーバーライドで変わった結果だけがここに残る。
        /// </summary>
        public static void ValidateExceptSource(
            Component component, string assetPath, string objectPath, List<ValidationIssue> issues)
        {
            Component source = PrefabUtility.GetCorrespondingObjectFromSource(component);
            if (source == null)
            {
                Validate(component, assetPath, objectPath, issues);
                return;
            }

            List<ValidationIssue> own = new List<ValidationIssue>();
            Validate(component, assetPath, objectPath, own);
            if (own.Count == 0) return;

            List<ValidationIssue> inherited = new List<ValidationIssue>();
            Validate(source, string.Empty, string.Empty, inherited);

            foreach (ValidationIssue issue in own)
            {
                if (!ContainsSameIssue(inherited, issue)) issues.Add(issue);
            }
        }

        private static bool ContainsSameIssue(List<ValidationIssue> issues, ValidationIssue target)
        {
            foreach (ValidationIssue issue in issues)
            {
                if (string.Equals(issue.PropertyPath, target.PropertyPath, StringComparison.Ordinal)
                    && issue.Message.Equals(target.Message))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
