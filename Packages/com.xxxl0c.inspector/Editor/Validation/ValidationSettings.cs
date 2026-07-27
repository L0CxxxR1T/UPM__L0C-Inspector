using UnityEditor;
using UnityEngine;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 検証の運用設定。ProjectSettings/ に保存するので Assets/ を汚さず、バージョン管理にも乗る。
    /// </summary>
    [FilePath("ProjectSettings/XXXL0CInspectorValidation.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class ValidationSettings : ScriptableSingleton<ValidationSettings>
    {
        [SerializeField] private bool _stopBuildOnError = true;
        [SerializeField] private bool _includeScenesInBuildCheck = true;

        /// <summary>
        /// ビルド前フックを有効にするか。
        /// 既定で true にしつつオフにできるようにするのは、締切直前に安全装置でビルドが
        /// 止まる事態を避けるため（外せると分かっている装置の方が結果的に長く使われる）。
        /// </summary>
        public bool StopBuildOnError
        {
            get => _stopBuildOnError;
            set => Set(ref _stopBuildOnError, value);
        }

        /// <summary>ビルド前チェックでシーンも見るか。シーンを開いて閉じる分、重い。</summary>
        public bool IncludeScenesInBuildCheck
        {
            get => _includeScenesInBuildCheck;
            set => Set(ref _includeScenesInBuildCheck, value);
        }

        private void Set(ref bool field, bool value)
        {
            if (field == value) return;

            field = value;
            Save(true);
        }
    }
}
