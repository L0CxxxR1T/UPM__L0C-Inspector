using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [InlineEditor] の装飾。Object 参照フィールドの下に、参照先のインスペクタをそのまま埋め込む。
    /// IMGUI 版の Editor キャッシュ管理は移植しない（UI Toolkit の InspectorElement のみ対応）。
    /// </summary>
    /// <remarks>
    /// A と B が互いを [InlineEditor] で参照すると埋め込みが無限に続くので、祖先の埋め込みをたどって
    /// 循環と深さを確かめる。祖先が確定するのはパネルに接続されてからなので、組み立てもそこまで遅らせる。
    /// </remarks>
    public sealed class InlineEditorDecorator : IPropertyDecorator
    {
        private const int MAX_DEPTH = 3;

        public Type AttributeType => typeof(InlineEditorAttribute);

        public int Order => 30;

        public void Decorate(DecorationContext context)
        {
            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
            {
                context.Row.Add(new HelpBox(
                    "[InlineEditor] は Object 参照フィールドにのみ使えます。", HelpBoxMessageType.Warning));
                return;
            }

            InlineEditorAttribute attribute = (InlineEditorAttribute)context.Attribute;
            SerializedObject serializedObject = context.Property.serializedObject;
            string propertyPath = context.Property.propertyPath;
            Object owner = serializedObject.targetObject;

            VisualElement container = new VisualElement();
            container.AddToClassList(InspectorClassNames.INLINE_EDITOR);
            context.Row.Add(container);

            bool built = false;
            Object shown = null;

            void Rebuild()
            {
                if (container.panel == null) return;

                // SerializedProperty は保持せずパスから引き直す。Undo や再シリアライズで stale になるため
                SerializedProperty property = serializedObject.FindProperty(propertyPath);
                Object target = property?.objectReferenceValue;
                if (built && target == shown) return;

                built = true;
                shown = target;
                container.Clear();
                container.userData = null;
                if (target == null) return;

                string blockReason = FindBlockReason(container, owner, target);
                if (blockReason != null)
                {
                    HelpBox notice = new HelpBox(blockReason, HelpBoxMessageType.Info);
                    notice.AddToClassList(InspectorClassNames.INLINE_EDITOR_NOTICE);
                    container.Add(notice);
                    return;
                }

                // 子の埋め込みが祖先として見つけられるよう、InspectorElement を足す前に印を付ける
                container.userData = new InlineTarget(owner, target);

                if (attribute.ShowOpenButton)
                {
                    container.Add(new Button(() => AssetDatabase.OpenAsset(target)) { text = "開く" });
                }

                container.Add(new InspectorElement(target));
            }

            container.RegisterCallback<AttachToPanelEvent>(_ => Rebuild());
            container.TrackPropertyValue(context.Property, _ => Rebuild());
        }

        /// <summary>埋め込むと循環する、または深すぎる場合にその理由を返す。問題なければ null。</summary>
        private static string FindBlockReason(VisualElement container, Object owner, Object target)
        {
            if (target == owner) return $"自分自身を参照しているため、ここでは展開しません（{target.name}）。";

            int depth = 0;
            for (VisualElement current = container.parent; current != null; current = current.parent)
            {
                if (!(current.userData is InlineTarget ancestor)) continue;
                if (!current.ClassListContains(InspectorClassNames.INLINE_EDITOR)) continue;

                depth++;
                if (ancestor.Owner == target || ancestor.Target == target)
                {
                    return $"循環参照になるため、ここでは展開しません（{target.name}）。";
                }
            }

            return depth >= MAX_DEPTH ? $"入れ子が {MAX_DEPTH} 段を超えるため、ここでは展開しません。" : null;
        }

        private sealed class InlineTarget
        {
            public Object Owner { get; }
            public Object Target { get; }

            public InlineTarget(Object owner, Object target)
            {
                Owner = owner;
                Target = target;
            }
        }
    }
}
