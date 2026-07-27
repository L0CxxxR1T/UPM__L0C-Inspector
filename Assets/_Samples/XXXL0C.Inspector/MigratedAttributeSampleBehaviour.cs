// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>段階4で移植した属性の動作確認用。</summary>
    public sealed class MigratedAttributeSampleBehaviour : MonoBehaviour
    {
        [Header("軽量デコレータ")]
        [Label("表示名を変更")]
        [SerializeField] private int _renamed;

        // 併用の実証：旧 PropertyDrawer 方式では Prefix と Suffix は同時に効かなかった
        [Prefix("x")]
        [Suffix("m/s")]
        [SerializeField] private float _speed = 5f;

        [ReadOnlyInPlayMode]
        [SerializeField] private string _configName = "既定値";

        [Header("IFieldFactory")]
        [SteppedRange(0f, 10f, 0.5f)]
        [SerializeField] private float _stepped;

        [SteppedMinMaxSlider(0f, 100f, 5f)]
        [SerializeField] private SteppedRangeSampleData _range;

        [SceneName]
        [SerializeField] private string _targetScene;

        [Header("OnValueChanged / InlineEditor")]
        [OnValueChanged(nameof(OnCountChanged))]
        [SerializeField] private int _count;

        // インスペクタで実際に呼ばれたかを見るためのフラグ
        [ReadOnly]
        [SerializeField] private int _onValueChangedCallCount;

        [InlineEditor]
        [SerializeField] private Material _inlineMaterial;

        [Header("全部併記（この設計の存在意義）")]
        [Label("盛り合わせ")]
        [Prefix("[")]
        [Suffix("]")]
        [ReadOnly]
        [Required]
        [SerializeField] private Transform _everything;

        private void OnCountChanged() => _onValueChangedCallCount++;
    }
}
