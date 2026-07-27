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
        public string DeclineReason { get; }

        public bool IsAccepted => DeclineReason == null;

        private FieldFactoryResult(VisualElement element, string declineReason)
        {
            Element = element;
            DeclineReason = declineReason;
        }

        public static FieldFactoryResult Accept(VisualElement element) => new FieldFactoryResult(element, null);

        public static FieldFactoryResult Decline(string reason) => new FieldFactoryResult(null, reason);
    }
}
