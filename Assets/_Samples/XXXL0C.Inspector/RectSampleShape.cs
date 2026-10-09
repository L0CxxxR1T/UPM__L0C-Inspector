// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    [Serializable]
    public sealed class RectSampleShape : ISampleShape
    {
        [SerializeField] private Vector2 _size = Vector2.one;
        [SerializeField] private Color _color = Color.white;
    }
}
