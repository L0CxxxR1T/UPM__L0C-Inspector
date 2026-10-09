using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// ビルド前に [Required] などのエラーが無いかを確認する。
    /// 対象はビルド対象シーンとその依存アセットに限定する（§13）。全アセット走査にしないのは、
    /// 未使用の作りかけプレハブで誤検知が出て警告全体が無視されるようになるのを防ぐため。
    /// </summary>
    public sealed class ValidationBuildHook : IPreprocessBuildWithReport
    {
        private const int MAX_LISTED_ISSUES = 10;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            ValidationSettings settings = ValidationSettings.instance;
            if (!settings.StopBuildOnError) return;

            List<ValidationIssue> issues = new List<ValidationIssue>();
            string[] scenePaths = BuildSceneList.GetEnabledScenePaths().ToArray();

            ValidateDependencies(scenePaths, issues);

            if (settings.IncludeScenesInBuildCheck)
            {
                ValidateScenes(scenePaths, issues);
            }

            int errorCount = issues.Count(issue => issue.Message.Severity == ValidationSeverity.Error);
            if (errorCount == 0) return;

            throw new BuildFailedException(BuildIssueMessage(issues, errorCount));
        }

        private static void ValidateDependencies(string[] scenePaths, List<ValidationIssue> issues)
        {
            HashSet<string> dependencyPaths = new HashSet<string>();
            foreach (string scenePath in scenePaths)
            {
                foreach (string dependency in AssetDatabase.GetDependencies(scenePath, true))
                {
                    dependencyPaths.Add(dependency);
                }
            }

            foreach (string path in dependencyPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
                    {
                        if (component == null) continue;

                        ObjectValidator.ValidateExceptSource(component, path, component.name, issues);
                    }

                    continue;
                }

                ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (scriptableObject != null)
                {
                    ObjectValidator.Validate(scriptableObject, path, string.Empty, issues);
                }
            }
        }

        private static void ValidateScenes(string[] scenePaths, List<ValidationIssue> issues)
        {
            // ダーティなシーンがあれば、開かずに止めて保存を促す。
            // ビルド中に保存確認ダイアログを出したり、変更を黙って捨てたりしない
            foreach (string scenePath in scenePaths)
            {
                Scene openScene = SceneManager.GetSceneByPath(scenePath);
                if (openScene.IsValid() && openScene.isDirty)
                {
                    throw new BuildFailedException(
                        $"シーン '{scenePath}' に未保存の変更があります。保存してからビルドしてください。");
                }
            }

            SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                foreach (string scenePath in scenePaths)
                {
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        foreach (Component component in root.GetComponentsInChildren<Component>(true))
                        {
                            if (component == null) continue;

                            ObjectValidator.ValidateExceptSource(component, scenePath, component.name, issues);
                        }
                    }
                }
            }
            finally
            {
                if (originalSetup != null && originalSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }
        }

        private static string BuildIssueMessage(List<ValidationIssue> issues, int errorCount)
        {
            IEnumerable<string> lines = issues
                .Where(issue => issue.Message.Severity == ValidationSeverity.Error)
                .Take(MAX_LISTED_ISSUES)
                .Select(issue => $"  {issue.AssetPath} / {issue.ComponentTypeName}: {issue.Message.Text}");

            string body = string.Join("\n", lines);
            string more = errorCount > MAX_LISTED_ISSUES ? $"\n  ...ほか {errorCount - MAX_LISTED_ISSUES} 件" : string.Empty;

            return $"[XXXL0C.Inspector] ビルド前検証でエラーが {errorCount} 件見つかりました。\n{body}{more}\n"
                + "Project Settings > XXXL0C > インスペクタ検証 でこのチェックを無効にできます。";
        }
    }
}
