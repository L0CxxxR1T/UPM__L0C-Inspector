using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[DisableInEditMode] の編集可否判定。</summary>
    public sealed class DisableInEditModeRule : IEnabledRule
    {
        public Type AttributeType => typeof(DisableInEditModeAttribute);

        public bool IsEnabled(VisibilityContext context) => EditorApplication.isPlaying;
    }
}
