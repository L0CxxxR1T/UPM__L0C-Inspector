using System;
using System.Collections.Generic;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [SceneName] の値がビルド対象のシーンにあるかを検証する。シーンの改名や削除で、
    /// ドロップダウンに無い名前が残ったまま実行時に読み込みが失敗するのを防ぐ。空文字は対象外。
    /// </summary>
    public sealed class SceneNameRule : IValidationRule
    {
        public Type AttributeType => typeof(SceneNameAttribute);

        public void Validate(ValidationContext context)
        {
            // String 以外は SceneNameFieldFactory が辞退理由を出す。Dictionary のキー・値も対象外
            if (context.Property.propertyType != SerializedPropertyType.String) return;
            if (context.Role != CollectionRole.None && context.Role != CollectionRole.Element) return;

            string sceneName = context.Property.stringValue;
            if (string.IsNullOrEmpty(sceneName)) return;

            List<string> sceneNames = BuildSceneList.GetEnabledSceneNames();
            if (sceneNames.Contains(sceneName)) return;

            context.Report(
                ValidationSeverity.Error,
                $"{context.Label} のシーン '{sceneName}' はビルド対象のシーンにありません。"
                + "Build Profiles のシーンリストを確認してください。");
        }
    }
}
