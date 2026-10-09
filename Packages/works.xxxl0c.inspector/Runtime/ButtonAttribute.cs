using System;

namespace XXXL0C.Inspector
{
    /// <summary>
    /// メソッドをインスペクタ上のボタンとして呼び出せるようにする。
    /// 対応する引数はインスペクタから入力でき、戻り値があれば結果を表示する。
    /// static メソッドは1回、instance メソッドは選択中の各オブジェクトで呼び出す。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary>ボタンの表示名。未指定ならメソッド名から自動生成する。</summary>
        public string Label { get; }

        public ButtonMode Mode { get; }

        /// <summary>
        /// 所属させるグループのパス。"/" 区切りで入れ子になる。未指定ならフッター
        /// （全フィールドの後ろ）に出る。既存の [BoxGroup] / [FoldoutGroup] のパスと同じ文字列を
        /// 指定すれば、そのグループの中に入る。
        /// </summary>
        public string Group { get; }

        public ButtonAttribute(string label = null, ButtonMode mode = ButtonMode.Always, string group = null)
        {
            Label = label;
            Mode = mode;
            Group = group;
        }
    }
}
