using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// プロジェクト横断の一括チェックウィンドウ。§13 の運用3層のうち2層目。
    /// 動的ロード（Addressables / Resources）で読まれるアセットの取りこぼしを、
    /// インスペクタを開かずに拾うためのもの。
    /// </summary>
    public sealed class ValidationWindow : EditorWindow
    {
        private const string STYLE_SHEET_PATH = "Packages/com.xxxl0c.inspector/Editor/Styles/ValidationWindow.uss";
        private const string STATE_NOT_RUN = "まだ実行していません。「実行」を押してください。";
        private const string STATE_NO_ISSUES = "問題は見つかりませんでした。";

        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();
        private readonly List<ValidationIssue> _filtered = new List<ValidationIssue>();

        private ListView _listView;
        private Label _summaryLabel;
        private Toggle _includeScenesToggle;
        private Toggle _errorsOnlyToggle;
        private bool _hasRun;

        [MenuItem("Window/XXXL0C/インスペクタ検証")]
        public static void Open() => GetWindow<ValidationWindow>("インスペクタ検証");

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_PATH);
            if (styleSheet != null) root.styleSheets.Add(styleSheet);

            VisualElement toolbar = new VisualElement();
            toolbar.AddToClassList("l0c-validation__toolbar");

            _includeScenesToggle = new Toggle("シーンも走査する") { value = true };
            Button runButton = new Button(Run) { text = "実行" };
            _errorsOnlyToggle = new Toggle("エラーのみ表示");
            _errorsOnlyToggle.RegisterValueChangedCallback(_ => ApplyFilter());

            toolbar.Add(runButton);
            toolbar.Add(_includeScenesToggle);
            toolbar.Add(_errorsOnlyToggle);
            root.Add(toolbar);

            _summaryLabel = new Label(STATE_NOT_RUN);
            _summaryLabel.AddToClassList("l0c-validation__summary");
            root.Add(_summaryLabel);

            _listView = new ListView(_filtered, -1, MakeItem, BindItem)
            {
                selectionType = SelectionType.Single
            };
            _listView.AddToClassList("l0c-validation__list");
            _listView.itemsChosen += items =>
            {
                foreach (object item in items)
                {
                    if (item is ValidationIssue issue) PingIssue(issue);
                    break;
                }
            };
            root.Add(_listView);
        }

        private void Run()
        {
            bool completed = ProjectValidator.ValidateProject(_includeScenesToggle.value, _issues);
            _hasRun = completed;

            if (!completed)
            {
                _summaryLabel.text = "キャンセルされました。";
                return;
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            _filtered.Clear();

            bool errorsOnly = _errorsOnlyToggle.value;
            int errorCount = 0;
            int warningCount = 0;

            foreach (ValidationIssue issue in _issues)
            {
                if (issue.Message.Severity == ValidationSeverity.Error) errorCount++;
                else if (issue.Message.Severity == ValidationSeverity.Warning) warningCount++;

                if (errorsOnly && issue.Message.Severity != ValidationSeverity.Error) continue;

                _filtered.Add(issue);
            }

            _summaryLabel.text = !_hasRun
                ? STATE_NOT_RUN
                : _issues.Count == 0
                    ? STATE_NO_ISSUES
                    : $"エラー {errorCount} 件 / 警告 {warningCount} 件";

            _listView.Rebuild();
        }

        private static VisualElement MakeItem()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("l0c-validation__row");

            Label severity = new Label { name = "severity" };
            severity.AddToClassList("l0c-validation__severity");
            Label location = new Label { name = "location" };
            location.AddToClassList("l0c-validation__location");
            Label message = new Label { name = "message" };
            message.AddToClassList("l0c-validation__message-text");

            row.Add(severity);
            row.Add(location);
            row.Add(message);
            return row;
        }

        private void BindItem(VisualElement element, int index)
        {
            ValidationIssue issue = _filtered[index];

            Label severity = element.Q<Label>("severity");
            severity.text = SeverityText(issue.Message.Severity);
            severity.EnableInClassList("l0c-validation__severity--error", issue.Message.Severity == ValidationSeverity.Error);
            severity.EnableInClassList("l0c-validation__severity--warning", issue.Message.Severity == ValidationSeverity.Warning);

            string location = string.IsNullOrEmpty(issue.ObjectPath)
                ? $"{issue.AssetPath} ({issue.ComponentTypeName})"
                : $"{issue.AssetPath} : {issue.ObjectPath} ({issue.ComponentTypeName})";

            element.Q<Label>("location").text = location;
            element.Q<Label>("message").text = issue.Message.Text;
        }

        private static string SeverityText(ValidationSeverity severity)
        {
            switch (severity)
            {
                case ValidationSeverity.Error: return "Error";
                case ValidationSeverity.Warning: return "Warning";
                default: return "Info";
            }
        }

        /// <summary>対象を選択する。シーン内オブジェクトなら、先にそのシーンを開く（保存確認あり）。</summary>
        private static void PingIssue(ValidationIssue issue)
        {
            if (!GlobalObjectId.TryParse(issue.GlobalObjectId, out GlobalObjectId id))
            {
                Debug.LogWarning($"[XXXL0C.Inspector] 対象を特定できませんでした: {issue.AssetPath}");
                return;
            }

            UnityEngine.Object target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            if (target == null)
            {
                // シーンが閉じている場合はここに落ちる。シーンを開いてから再取得する
                if (!TryOpenContainingScene(issue.AssetPath)) return;

                target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                if (target == null)
                {
                    Debug.LogWarning($"[XXXL0C.Inspector] シーンを開きましたが対象が見つかりませんでした: {issue.AssetPath}");
                    return;
                }
            }

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private static bool TryOpenContainingScene(string assetPath)
        {
            if (!assetPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) return false;

            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                assetPath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            return true;
        }
    }
}
