// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
  /// <summary>Object 参照制約と Edit / Play Mode 表示の動作確認用。</summary>
  public sealed class ReferenceModeSampleBehaviour : MonoBehaviour
  {
    [AssetsOnly]
    [PreviewField]
    [SerializeField] private Material _assetMaterial;

    [SceneObjectsOnly]
    [SerializeField] private GameObject _sceneObject;

    [ChildGameObjectsOnly]
    [SerializeField] private Transform _childTransform;

    [HideInPlayMode]
    [SerializeField] private string _editModeNote = "Play Mode 中は非表示です";

    [DisableInEditMode]
    [SerializeField] private int _runtimeOnlyValue;
  }
}