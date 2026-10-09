using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [PreviewField] の装飾。Object 参照フィールドの右に参照先のプレビューを出す。
    /// Texture / Sprite はそのまま、それ以外は AssetPreview の画像（読み込み中はミニサムネイル）を使う。
    /// </summary>
    public sealed class PreviewFieldDecorator : IPropertyDecorator
    {
        // AssetPreview は非同期に作られるので、できるまで間隔を空けて取り直す。シーン上のオブジェクトなど
        // プレビューが作られないものもあるので、回数で打ち切る
        private const long POLL_INTERVAL_MS = 100;
        private const int MAX_POLL_COUNT = 50;

        public Type AttributeType => typeof(PreviewFieldAttribute);

        // [Prefix] / [Suffix] より後に並べる
        public int Order => 15;

        public void Decorate(DecorationContext context)
        {
            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
            {
                context.Row.Add(new HelpBox(
                    "[PreviewField] は Object 参照フィールドにのみ使えます。", HelpBoxMessageType.Warning));
                return;
            }

            PreviewFieldAttribute attribute = (PreviewFieldAttribute)context.Attribute;

            Image image = new Image { scaleMode = ScaleMode.ScaleToFit };
            image.AddToClassList(InspectorClassNames.PREVIEW);

            // 大きさは属性の引数なので USS に逃がせない
            image.style.width = attribute.Size;
            image.style.height = attribute.Size;

            AffixRowUtility.GetOrCreateRow(context).Add(image);

            SerializedObject serializedObject = context.Property.serializedObject;
            string propertyPath = context.Property.propertyPath;
            IVisualElementScheduledItem polling = null;

            void Refresh()
            {
                polling?.Pause();

                SerializedProperty property = serializedObject.FindProperty(propertyPath);
                Object target = property == null || property.hasMultipleDifferentValues
                    ? null
                    : property.objectReferenceValue;

                if (ApplyPreview(image, target)) return;

                int remaining = MAX_POLL_COUNT;
                polling = image.schedule.Execute(() =>
                {
                    remaining--;
                    if (ApplyPreview(image, target) || remaining <= 0) polling.Pause();
                }).Every(POLL_INTERVAL_MS);
            }

            image.TrackPropertyValue(context.Property, _ => Refresh());
            Refresh();
        }

        /// <summary>プレビューを当てる。まだ作られていなければ代わりの画像を当てて false を返す。</summary>
        private static bool ApplyPreview(Image image, Object target)
        {
            image.sprite = null;
            image.image = null;

            switch (target)
            {
                case null:
                    return true;
                case Sprite sprite:
                    image.sprite = sprite;
                    return true;
                case Texture texture:
                    image.image = texture;
                    return true;
            }

            Texture2D preview = AssetPreview.GetAssetPreview(target);
            if (preview != null)
            {
                image.image = preview;
                return true;
            }

            image.image = AssetPreview.GetMiniThumbnail(target);
            return false;
        }
    }
}
