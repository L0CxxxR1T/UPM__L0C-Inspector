// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>Optional / EnumIndexedList / SerializableType の表示確認用。</summary>
    public sealed class TypeSampleBehaviour : MonoBehaviour
    {
        [Header("Optional")]
        [SerializeField] private Optional<int> _overrideCount = Optional<int>.Some(3);
        [SerializeField] private Optional<Material> _overrideMaterial;

        [Header("EnumIndexedList（6.5 以前向け）")]
        [SerializeField] private EnumIndexedList<DictionarySampleKind, float> _resistances =
            new EnumIndexedList<DictionarySampleKind, float>();

        [Header("SerializableType")]
        [Required]
        [TypeFilter(typeof(ISampleShape))]
        [SerializeField] private SerializableType _shapeType;

        [TypeFilter(typeof(ISampleShape))]
        [SerializeField] private List<SerializableType> _shapeTypes = new List<SerializableType>();

        // 基底型の指定が無いと選べない（検証層が Warning を出す）
        [TypeFilter]
        [SerializeField] private SerializableType _unfilteredType;
    }
}
