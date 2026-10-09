using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>[SceneName] のフィールドファクトリ。string フィールドを Build Settings のシーン名ドロップダウンにする。</summary>
    public sealed class SceneNameFieldFactory : IFieldFactory
    {
        public Type AttributeType => typeof(SceneNameAttribute);

        public FieldFactoryResult Create(FieldFactoryContext context)
        {
            if (context.Property.propertyType != SerializedPropertyType.String)
            {
                return FieldFactoryResult.Decline(
                    $"[SceneName] は String 型にのみ使えます（{context.Property.propertyType} には使えません）。");
            }

            List<string> sceneNames = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                sceneNames.Add(Path.GetFileNameWithoutExtension(scene.path));
            }

            if (sceneNames.Count == 0)
            {
                return FieldFactoryResult.Decline("[SceneName] を使うには Build Settings にシーンを登録してください。");
            }

            DropdownField dropdown = new DropdownField(context.Property.displayName, sceneNames, 0);
            dropdown.BindProperty(context.Property);

            return FieldFactoryResult.Accept(dropdown);
        }
    }
}
