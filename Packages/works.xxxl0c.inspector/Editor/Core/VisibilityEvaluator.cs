using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 可視性ルールと編集可否ルールを評価する。
    /// </summary>
    /// <remarks>
    /// UI に依存しないのは意図的。インスペクタ・一括チェック・ビルド前フックの3層が同じ判定を通ることで
    /// 結果が一致する。ここがズレると「インスペクタでは何も出ないのにビルドが止まる」状態になり、
    /// 誤検知として扱われて警告全体が無視されるようになる。
    /// </remarks>
    public static class VisibilityEvaluator
    {
        /// <summary>属性をその場で取り出して評価する。走査系（1回きりの呼び出し）向け。</summary>
        public static bool IsVisible(
            SerializedProperty property,
            FieldInfo fieldInfo,
            string label,
            List<ValidationMessage> messages)
        {
            if (fieldInfo == null) return true;

            return Evaluate(property, fieldInfo, fieldInfo.GetCustomAttributes(false), label, messages, IsVisibleByRule);
        }

        /// <summary>
        /// 構築時にキャッシュした属性で評価する。更新パスは変更ごとに走るのでリフレクションを持ち込まない。
        /// </summary>
        public static bool IsVisible(
            SerializedProperty property,
            FieldInfo fieldInfo,
            Attribute[] attributes,
            string label,
            List<ValidationMessage> messages)
            => Evaluate(property, fieldInfo, attributes, label, messages, IsVisibleByRule);

        /// <summary>編集できるか。属性をその場で取り出して評価する。走査系（1回きりの呼び出し）向け。</summary>
        public static bool IsEnabled(
            SerializedProperty property,
            FieldInfo fieldInfo,
            string label,
            List<ValidationMessage> messages)
        {
            if (fieldInfo == null) return true;

            return Evaluate(property, fieldInfo, fieldInfo.GetCustomAttributes(false), label, messages, IsEnabledByRule);
        }

        /// <summary>編集できるか。構築時にキャッシュした属性で評価する。</summary>
        public static bool IsEnabled(
            SerializedProperty property,
            FieldInfo fieldInfo,
            Attribute[] attributes,
            string label,
            List<ValidationMessage> messages)
            => Evaluate(property, fieldInfo, attributes, label, messages, IsEnabledByRule);

        /// <summary>
        /// ルールの AND。false を返すルールがあっても残りを走らせるのは、誤用の報告を取りこぼさないため。
        /// </summary>
        private static bool Evaluate(
            SerializedProperty property,
            FieldInfo fieldInfo,
            IEnumerable<object> attributes,
            string label,
            List<ValidationMessage> messages,
            Func<Attribute, VisibilityContext, bool?> evaluateRule)
        {
            bool result = true;

            foreach (object candidate in attributes)
            {
                Attribute attribute = candidate as Attribute;
                if (attribute == null) continue;

                VisibilityContext context = new VisibilityContext(property, fieldInfo, attribute, label, messages);
                if (evaluateRule(attribute, context) == false) result = false;
            }

            return result;
        }

        /// <summary>担当するルールが無ければ null。</summary>
        private static bool? IsVisibleByRule(Attribute attribute, VisibilityContext context)
            => ExtensionRegistry.FindVisibilityRule(attribute.GetType())?.IsVisible(context);

        private static bool? IsEnabledByRule(Attribute attribute, VisibilityContext context)
            => ExtensionRegistry.FindEnabledRule(attribute.GetType())?.IsEnabled(context);
    }
}
