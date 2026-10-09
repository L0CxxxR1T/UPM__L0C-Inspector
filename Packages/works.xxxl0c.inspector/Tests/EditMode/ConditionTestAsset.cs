// テスト用の入れ物なので、C# からは参照しないフィールドがある
#pragma warning disable CS0169

using System;
using UnityEngine;

namespace XXXL0C.Inspector.Tests.EditMode
{
    /// <summary>ConditionEvaluatorTest 用の SerializedObject の元。</summary>
    internal sealed class ConditionTestAsset : ScriptableObject
    {
        [SerializeField] private bool _flag;
        [SerializeField] private ConditionTestKind _kind;
        [SerializeField] private int _count;
        [SerializeField] private float _ratio;
        [SerializeField] private string _label;
        [SerializeField] private Material _material;
        [SerializeField] private Nested _nested;
        [SerializeField] private Nested[] _items;

        [Serializable]
        internal struct Nested
        {
            [SerializeField] private bool _enabled;
            [SerializeField] private int _value;
        }
    }

    internal enum ConditionTestKind
    {
        First,
        Second
    }
}
