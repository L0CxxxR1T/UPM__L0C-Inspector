using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [ReadOnlyInPlayMode] の装飾。Play Mode 中だけ編集を禁止する。
    /// 旧実装は構築時の Application.isPlaying しか見ておらず、インスペクタを開いたまま
    /// Play Mode に入っても追従しなかった。PlayModeTracking で追従させる。
    /// </summary>
    public sealed class ReadOnlyInPlayModeDecorator : IPropertyDecorator
    {
        public Type AttributeType => typeof(ReadOnlyInPlayModeAttribute);

        public int Order => 20;

        public void Decorate(DecorationContext context)
        {
            VisualElement field = context.Field;
            PlayModeTracking.Track(field, () => field.SetEnabled(!EditorApplication.isPlaying));
        }
    }
}
