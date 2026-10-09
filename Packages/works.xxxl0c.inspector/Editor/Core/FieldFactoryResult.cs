using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// IFieldFactory の結果。型では機能しない場合は黙って標準 PropertyField に戻さず、
    /// 理由を添えて辞退する（§13：静かに無視すると「付けたつもりで守られてない」状態が生まれる）。
    /// </summary>
    public readonly struct FieldFactoryResult
    {
        public VisualElement Element { get; }

        /// <summary>表示名を出している Label。[Label] などの装飾が書き換える。無ければ null。</summary>
        public Label LabelElement { get; }

        public string DeclineReason { get; }

        public bool IsAccepted => DeclineReason == null;

        private FieldFactoryResult(VisualElement element, Label labelElement, string declineReason)
        {
            Element = element;
            LabelElement = labelElement;
            DeclineReason = declineReason;
        }

        /// <param name="labelElement">
        /// 表示名の Label。BaseField なら labelElement を渡す。渡さないと [Label] が反映されない。
        /// </param>
        public static FieldFactoryResult Accept(VisualElement element, Label labelElement)
            => new FieldFactoryResult(element, labelElement, null);

        public static FieldFactoryResult Decline(string reason) => new FieldFactoryResult(null, null, reason);
    }
}
