using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[HideInPlayMode] の可視性判定。</summary>
    public sealed class HideInPlayModeRule : IVisibilityRule
    {
        public Type AttributeType => typeof(HideInPlayModeAttribute);

        public bool IsVisible(VisibilityContext context) => !EditorApplication.isPlaying;
    }
}
