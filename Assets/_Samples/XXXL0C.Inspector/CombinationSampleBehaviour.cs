// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>本体差し替え属性と装飾属性の併記、ReadOnly、TypeFilter の動作確認用。</summary>
    public sealed class CombinationSampleBehaviour : MonoBehaviour
    {
        [Header("本体差し替え + 装飾")]
        [Tooltip("差し替えた本体にもツールチップが出ます。")]
        [Label("音量")]
        [OnValueChanged(nameof(OnVolumeChanged))]
        [SteppedRange(0f, 1f, 0.1f)]
        [SerializeField] private float _volume = 0.5f;

        // OnValueChanged が呼ばれた回数。複数選択して変えると、選択した全部で増える
        [ReadOnly]
        [SerializeField] private int _volumeChangedCount;

        [Label("開始シーン")]
        [SceneName]
        [SerializeField] private string _startScene;

        // ビルド対象に無いシーン名は検証層で Error になる
        [SceneName]
        [SerializeField] private string _missingScene = "DeletedScene";

        [Label("出現間隔（秒）")]
        [SteppedMinMaxSlider(0f, 10f, 0.5f)]
        [SerializeField] private FloatRange _interval = new FloatRange(1f, 3f);

        [SteppedMinMaxSlider(1f, 20f, 1f)]
        [SerializeField] private IntRange _spawnCount = new IntRange(2, 5);

        [Header("ReadOnly でも折りたたみは開ける")]
        [ReadOnly]
        [SerializeField] private List<Vector3> _lockedPoints = new List<Vector3> { Vector3.zero, Vector3.one };

        // 2つの ReadOnly 系が打ち消し合わない（Edit Mode でも編集できない）
        [ReadOnly]
        [ReadOnlyInPlayMode]
        [SerializeField] private string _doubleLocked = "編集できません";

        [Header("TypeFilter")]
        [Required]
        [TypeFilter]
        [SerializeReference] private ISampleShape _shape;

        private void OnVolumeChanged() => _volumeChangedCount++;
    }
}
