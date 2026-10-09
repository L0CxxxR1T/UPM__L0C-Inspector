using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// ビルド対象のシーン一覧。[SceneName] の候補・検証とビルド前フックが同じ一覧を見るよう1箇所に集約する。
    /// </summary>
    /// <remarks>
    /// EditorBuildSettings.scenes は、アクティブな Build Profile がシーンリストを上書きしていればそちらを返す。
    /// アクティブでないプロファイルを指定したビルドや、スクリプトからシーンを直接渡したビルドは判別できない。
    /// </remarks>
    internal static class BuildSceneList
    {
        public static List<string> GetEnabledScenePaths()
        {
            List<string> paths = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled) paths.Add(scene.path);
            }

            return paths;
        }

        /// <summary>SceneManager.LoadScene に渡す名前（拡張子なしのファイル名）。重複は1つにまとめる。</summary>
        public static List<string> GetEnabledSceneNames()
        {
            List<string> names = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string path in GetEnabledScenePaths())
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (seen.Add(name)) names.Add(name);
            }

            return names;
        }
    }
}
