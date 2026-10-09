// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
  /// <summary>編集条件と値の検証の動作確認用。</summary>
  public sealed class ConditionValidationSampleBehaviour : MonoBehaviour
  {
    [SerializeField] private bool _advancedSettings;

    [EnableIf(nameof(_advancedSettings))]
    [SerializeField] private int _advancedValue = 4;

    [DisableIf(nameof(_advancedSettings))]
    [SerializeField] private string _basicValue = "基本設定中に編集できます";

    [NotEmpty]
    [SerializeField] private string _displayName = "サンプル";

    [NotEmpty]
    [SerializeField] private List<int> _waypoints = new List<int> { 1, 2, 3 };

    [MinValue(1)]
    [SerializeField] private int _minimumCount = 1;

    [MaxValue(100)]
    [SerializeField] private float _maximumSpeed = 10f;
  }
}