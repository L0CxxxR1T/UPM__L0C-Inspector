// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>グループ・バッジ伝播・空グループ隠しの動作確認用。</summary>
    public sealed class GroupSampleBehaviour : MonoBehaviour
    {
        // グループ名はマジックストリングを散らさないよう const にまとめる
        private const string GROUP_VIEW = "表示";
        private const string GROUP_VIEW_COLOR = "表示/色";
        private const string GROUP_BATTLE = "戦闘";
        private const string GROUP_BATTLE_DAMAGE = "戦闘/ダメージ";
        private const string GROUP_OPTIONAL = "任意設定";

        [BoxGroup(GROUP_VIEW)]
        [SerializeField] private string _displayName = "サンプル";

        // 入れ子のグループ。パスの途中セグメントは最初に必要とした種類で作られる
        [BoxGroup(GROUP_VIEW_COLOR)]
        [SerializeField] private Color _baseColor = Color.white;

        [BoxGroup(GROUP_VIEW_COLOR)]
        [SerializeField] private Color _accentColor = Color.cyan;

        // 未設定なので「戦闘」ヘッダにエラーバッジが出る
        [FoldoutGroup(GROUP_BATTLE)]
        [Required]
        [SerializeField] private Collider _hitBox;

        // 入れ子なので「ダメージ」にも「戦闘」にもバッジが伝播する
        [FoldoutGroup(GROUP_BATTLE_DAMAGE)]
        [Required]
        [SerializeField] private Material _hitEffect;

        [FoldoutGroup(GROUP_BATTLE_DAMAGE)]
        [SerializeField, Min(0)] private int _damage = 10;

        [SerializeField] private bool _showOptional;

        // この2つが両方消えると「任意設定」グループごと消える
        [FoldoutGroup(GROUP_OPTIONAL)]
        [ShowIf(nameof(_showOptional))]
        [SerializeField] private float _fadeSeconds = 0.2f;

        [FoldoutGroup(GROUP_OPTIONAL)]
        [ShowIf(nameof(_showOptional))]
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        // グループに入れないフィールド。宣言順どおりグループの後に出る
        [SerializeField] private bool _enabledOnStart = true;
    }
}
