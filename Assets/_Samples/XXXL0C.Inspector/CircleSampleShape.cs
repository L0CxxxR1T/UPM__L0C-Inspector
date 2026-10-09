// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using System;
using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    [Serializable]
    public sealed class CircleSampleShape : ISampleShape
    {
        [SerializeField] private float _radius = 1f;
    }
}
