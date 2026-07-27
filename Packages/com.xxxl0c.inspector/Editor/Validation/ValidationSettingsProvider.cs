using UnityEditor;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>Project Settings > XXXL0C > インスペクタ検証 に設定項目を出す。</summary>
    internal static class ValidationSettingsProvider
    {
        private const string PATH = "Project/XXXL0C/インスペクタ検証";

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(PATH, SettingsScope.Project)
            {
                label = "インスペクタ検証",
                activateHandler = (searchContext, root) =>
                {
                    ValidationSettings settings = ValidationSettings.instance;

                    Toggle stopOnErrorToggle = new Toggle("ビルド時にエラーで止める")
                    {
                        value = settings.StopBuildOnError,
                        tooltip = "ビルド対象シーンとその依存アセットに [Required] などのエラーがあると、"
                            + "ビルド前にエラーで止めます。締切直前に急いでオフにできるよう、"
                            + "設定として残してあります。"
                    };
                    stopOnErrorToggle.RegisterValueChangedCallback(
                        evt => settings.StopBuildOnError = evt.newValue);

                    Toggle includeScenesToggle = new Toggle("ビルド時にシーンも検証する")
                    {
                        value = settings.IncludeScenesInBuildCheck,
                        tooltip = "ビルド対象シーンを開いて検証します。プレハブ・ScriptableObject の検証より"
                            + "時間が掛かります。"
                    };
                    includeScenesToggle.RegisterValueChangedCallback(
                        evt => settings.IncludeScenesInBuildCheck = evt.newValue);

                    root.Add(stopOnErrorToggle);
                    root.Add(includeScenesToggle);
                }
            };
        }
    }
}
