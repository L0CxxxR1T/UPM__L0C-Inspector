using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// プロジェクト全体（プレハブ・ScriptableObject・シーン）を走査して検証する。
    /// §13 が「動的ロード（Addressables / Resources）の取りこぼしはここで拾う」と書いている本来の用途。
    /// </summary>
    public static class ProjectValidator
    {
        private const string PROGRESS_TITLE = "インスペクタ検証";

        /// <summary>
        /// プロジェクト全体を走査する。includeScenes が true ならシーンも開いて検証する
        /// （重いのでキャンセル可能な進捗バーを出す）。
        /// </summary>
        /// <returns>ユーザーがキャンセルした、またはシーンの保存確認でキャンセルされたら false。</returns>
        public static bool ValidateProject(bool includeScenes, List<ValidationIssue> issues)
        {
            issues.Clear();

            try
            {
                if (!ValidatePrefabs(issues)) return false;
                if (!ValidateScriptableObjects(issues)) return false;
                if (includeScenes && !ValidateScenes(issues)) return false;

                return true;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool ValidatePrefabs(List<ValidationIssue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < guids.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                        PROGRESS_TITLE, $"プレハブを検証中 ({i + 1}/{guids.Length})", (float)i / guids.Length))
                {
                    return false;
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                // エディタツールがアセット全体を走査する正当な用途。
                // ランタイムの View 配線で GetComponent を避ける方針とは別の話
                foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
                {
                    if (component == null) continue; // Missing Script

                    ObjectValidator.ValidateExceptSource(component, path, BuildHierarchyPath(component.transform), issues);
                }
            }

            return true;
        }

        private static bool ValidateScriptableObjects(List<ValidationIssue> issues)
        {
            // t:ScriptableObject は Unity 内部のアセットも大量に拾うが、ObjectValidator のゲートで
            // 属性を持たないものは即 return するので実害と負荷は無い
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            for (int i = 0; i < guids.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar(
                        PROGRESS_TITLE, $"ScriptableObject を検証中 ({i + 1}/{guids.Length})", (float)i / guids.Length))
                {
                    return false;
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                ObjectValidator.Validate(asset, path, string.Empty, issues);
            }

            return true;
        }

        private static bool ValidateScenes(List<ValidationIssue> issues)
        {
            // 未保存の変更を持ったまま走査を始めない。キャンセルされたら走査自体を止める
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            SceneSetup[] originalSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                string[] guids = AssetDatabase.FindAssets("t:Scene");
                for (int i = 0; i < guids.Length; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            PROGRESS_TITLE, $"シーンを検証中 ({i + 1}/{guids.Length})", (float)i / guids.Length))
                    {
                        return false;
                    }

                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        foreach (Component component in root.GetComponentsInChildren<Component>(true))
                        {
                            if (component == null) continue;

                            ObjectValidator.ValidateExceptSource(component, path, BuildHierarchyPath(component.transform), issues);
                        }
                    }
                }

                return true;
            }
            finally
            {
                // 走査前の構成へ必ず戻す。ここを finally の外に出すと、キャンセル時に元のシーン構成が失われる
                if (originalSetup != null && originalSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            if (transform.parent == null) return transform.name;

            return BuildHierarchyPath(transform.parent) + "/" + transform.name;
        }
    }
}
