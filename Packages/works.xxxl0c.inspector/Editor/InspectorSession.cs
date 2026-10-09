using System;
using System.Collections.Generic;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 構築済みツリーに紐付く更新パス。可視性 → 検証 → バッジ伝播 / 空グループ判定 の順に走る。
    /// </summary>
    internal sealed class InspectorSession
    {
        private readonly SerializedObject _serializedObject;
        private readonly List<InspectorRow> _rows = new List<InspectorRow>();
        private readonly List<ValidationMessage> _buffer = new List<ValidationMessage>();
        private readonly Dictionary<string, InspectorRow> _rowsByPath =
            new Dictionary<string, InspectorRow>(StringComparer.Ordinal);
        private readonly Predicate<string> _hasOwnRow;

        private GroupNode _groupRoot;

        public InspectorSession(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            _hasOwnRow = path => _rowsByPath.ContainsKey(path);
        }

        public void SetGroupRoot(GroupNode groupRoot) => _groupRoot = groupRoot;

        /// <summary>
        /// 行を登録する。展開したネストクラスは親→子の順に登録される前提で、
        /// 更新パスは親の非表示を子に引き継ぐ。
        /// </summary>
        public void Register(InspectorRow row)
        {
            _rows.Add(row);
            _rowsByPath[row.PropertyPath] = row;
        }

        public void Refresh()
        {
            // 複数選択時は混在値で誤検知が出る。誤検知は警告全体の信頼を落とすので出さない側を選ぶ
            bool multiEditing = _serializedObject.targetObjects.Length > 1;

            foreach (InspectorRow row in _rows)
            {
                _buffer.Clear();

                // SerializedProperty はキャッシュせずパスから引き直す。Undo や再シリアライズで stale になるため
                SerializedProperty property = _serializedObject.FindProperty(row.PropertyPath);

                // 親が隠れているなら子も評価しない。隠れた親の中身を未設定だと責めても直せない
                bool visible = IsParentVisible(row.PropertyPath)
                    && (property == null
                        || VisibilityEvaluator.IsVisible(property, row.FieldInfo, row.Attributes, row.Label, _buffer));
                row.SetVisible(visible);

                // 非表示のフィールドは検証しない（設定できないものを未設定だと責めない）
                if (visible && !multiEditing && property != null)
                {
                    ValidationWalker.Collect(property, row.FieldInfo, _buffer, _hasOwnRow);
                }

                if (!row.MatchesMessages(_buffer)) row.ApplyMessages(_buffer);
            }

            _groupRoot?.Update();
        }

        /// <summary>
        /// 自前展開したネストクラスの親行が非表示なら false。トップレベルの行は常に true。
        /// </summary>
        private bool IsParentVisible(string propertyPath)
        {
            string parentPath = PropertyPathUtility.GetParentPath(propertyPath);
            if (parentPath.Length == 0) return true;

            return !_rowsByPath.TryGetValue(parentPath, out InspectorRow parent) || parent.IsVisible;
        }
    }
}
