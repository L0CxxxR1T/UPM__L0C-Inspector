using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[HideInEditMode] の可視性判定。</summary>
    public sealed class HideInEditModeRule : IVisibilityRule
    {
        public Type AttributeType => typeof(HideInEditModeAttribute);

        public bool IsVisible(VisibilityContext context) => EditorApplication.isPlaying;
    }
}
