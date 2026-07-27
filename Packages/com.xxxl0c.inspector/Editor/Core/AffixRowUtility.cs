using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [Prefix] / [Suffix] が共有する、本体を横並びコンテナで包む処理。
    /// 両方が同じフィールドに付いても、どちらが先に走っても同じ見た目になるよう
    /// 「先頭に挿入」「末尾に追加」の絶対位置だけで組み立てる。
    /// </summary>
    internal static class AffixRowUtility
    {
        /// <summary>既に横並びコンテナがあれば再利用し、無ければ本体を包んで作る。</summary>
        public static VisualElement GetOrCreateRow(DecorationContext context)
        {
            VisualElement existing = context.Row.Q(className: InspectorClassNames.AFFIX_ROW);
            if (existing != null) return existing;

            VisualElement affixRow = new VisualElement();
            affixRow.AddToClassList(InspectorClassNames.AFFIX_ROW);

            int fieldIndex = context.Row.IndexOf(context.Field);
            context.Row.Insert(fieldIndex, affixRow);
            context.Row.Remove(context.Field);
            affixRow.Add(context.Field);

            // 横並びの中で本体が余白を埋めるように広げる
            context.Field.style.flexGrow = 1;

            return affixRow;
        }

        public static Label CreateAffixLabel(string text)
        {
            Label label = new Label(text);
            label.AddToClassList(InspectorClassNames.AFFIX_LABEL);
            return label;
        }
    }
}
