// サンプルなので、参照していないフィールドがある
#pragma warning disable CS0169, CS0414

using UnityEngine;

namespace XXXL0C.Inspector.Samples
{
    /// <summary>[Button] の動作確認用。</summary>
    public sealed class ButtonSampleBehaviour : MonoBehaviour
    {
        [SerializeField] private int _clickCount;

        // フッター（全フィールドの後ろ）に出る。ラベル未指定なのでメソッド名から自動生成される
        [Button]
        private void IncrementClickCount() => _clickCount++;

        // ラベル指定 + 既存グループに便乗
        [BoxGroup("操作")]
        [SerializeField] private string _note = "";

        [Button("カウントをリセット", group: "操作")]
        private void ResetClickCount() => _clickCount = 0;

        // Play Mode 中だけ押せる
        [Button("実行時だけ有効", ButtonMode.PlayModeOnly)]
        private void PlayModeOnlyAction() => Debug.Log("[XXXL0C.Inspector] PlayModeOnlyAction を実行しました。");

        // EditMode 中だけ押せる
        [Button("編集中だけ有効", ButtonMode.EditorOnly)]
        private void EditorOnlyAction() => Debug.Log("[XXXL0C.Inspector] EditorOnlyAction を実行しました。");

        // ref 引数は対象外なので notice が出る
        [Button]
        private void Misused(ref int _unused)
        {
        }

        // ボタンだけのグループ。フィールドが1つも無くても隠れないことの確認用
        [Button(group: "ボタンのみ")]
        private void ButtonOnlyGroupAction() => Debug.Log("[XXXL0C.Inspector] ButtonOnlyGroupAction を実行しました。");

        [Button("指定回数を加算", group: "操作")]
        [ButtonGroup("カウント")]
        private int AddCount(int amount = 2)
        {
            _clickCount += amount;
            return _clickCount;
        }

        [Button("カウントを説明", group: "操作")]
        [ButtonGroup("カウント")]
        private string DescribeCount(string prefix = "現在のカウント") => $"{prefix}: {_clickCount}";

        [Button("static の挨拶")]
        private static string StaticGreeting(string name = "Inspector") => $"こんにちは、{name}さん";

        [Button("引数の型サンプル")]
        private Vector3 ParameterTypes(
            long count = 3,
            double ratio = 0.5,
            bool enabled = true,
            Vector2 offset = default,
            Vector2Int cell = default,
            Color color = default,
            UnityEngine.Object asset = null)
            => new Vector3(count * (float)ratio, enabled ? offset.x : 0f, cell.y + color.a + (asset == null ? 0f : 1f));
    }
}
