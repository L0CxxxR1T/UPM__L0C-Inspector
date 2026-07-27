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

        // 引数ありメソッドに付けた誤用。標準のボタンにはならず、notice が出る
        [Button]
        private void Misused(int _unused)
        {
        }

        // ボタンだけのグループ。フィールドが1つも無くても隠れないことの確認用
        [Button(group: "ボタンのみ")]
        private void ButtonOnlyGroupAction() => Debug.Log("[XXXL0C.Inspector] ButtonOnlyGroupAction を実行しました。");
    }
}
