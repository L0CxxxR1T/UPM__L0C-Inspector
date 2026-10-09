using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [SceneName] のフィールドファクトリ。string フィールドをビルド対象のシーン名のドロップダウンにする。
    /// 一覧に無い名前が入っている場合は <see cref="SceneNameRule"/> が検証層で報告する。
    /// </summary>
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

            List<string> sceneNames = BuildSceneList.GetEnabledSceneNames();
            if (sceneNames.Count == 0)
            {
                return FieldFactoryResult.Decline("[SceneName] を使うには Build Profiles のシーンリストにシーンを登録してください。");
            }

            DropdownField dropdown = new DropdownField(context.Property.displayName, sceneNames, 0);
            dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            dropdown.BindProperty(context.Property);

            return FieldFactoryResult.Accept(dropdown, dropdown.labelElement);
        }
    }
}
