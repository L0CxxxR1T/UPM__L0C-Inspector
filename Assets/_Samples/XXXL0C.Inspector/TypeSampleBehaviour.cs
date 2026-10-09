// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System;
using System.Collections.Generic;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>Optional / TypeFilter / enum / preview / ValueDropdown の表示確認用。</summary>
    public sealed class TypeSampleBehaviour : MonoBehaviour
    {
        private static readonly ValueDropdownList<int> SPEED_OPTIONS = new ValueDropdownList<int>
        {
            { "ゆっくり", 1 },
            { "ふつう", 3 },
            { "速い", 5 }
        };

        [Flags]
        private enum SampleOptions
        {
            None = 0,
            Move = 1 << 0,
            Rotate = 1 << 1,
            Scale = 1 << 2
        }

        [Header("Optional")]
        [SerializeField] private Optional<int> _overrideCount = Optional<int>.Some(3);
        [SerializeField] private Optional<Material> _overrideMaterial;

        [Header("EnumIndexedList（6.5 以前向け）")]
        [SerializeField]
        private EnumIndexedList<DictionarySampleKind, float> _resistances =
            new EnumIndexedList<DictionarySampleKind, float>();

        [Header("SerializableType")]
        [Required]
        [TypeFilter(typeof(ISampleShape))]
        [SerializeField] private SerializableType _shapeType;

        [TypeFilter(typeof(ISampleShape))]
        [SerializeField] private List<SerializableType> _shapeTypes = new List<SerializableType>();

        [Header("SerializeReference")]
        [TypeFilter(typeof(ISampleShape))]
        [SerializeReference] private ISampleShape[] _shapeArray = Array.Empty<ISampleShape>();

        [TypeFilter(typeof(ISampleShape))]
        [SerializeReference] private List<ISampleShape> _shapes = new List<ISampleShape>();

        // 基底型の指定が無いと選べない（検証層が Warning を出す）
        [TypeFilter]
        [SerializeField] private SerializableType _unfilteredType;

        [Header("enum / preview / dropdown")]
        [EnumToggleButtons]
        [SerializeField] private SampleOptions _options;

        [PreviewField(72f)]
        [SerializeField] private Texture2D _texturePreview;

        [ValueDropdown(nameof(SPEED_OPTIONS))]
        [SerializeField] private int _speed = 3;
    }
}
