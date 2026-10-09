using System;
using UnityEditor;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// propertyPath の操作を1箇所に集約する。
    /// 兄弟プロパティは必ず親パスからの相対で引く。ルートから名前で探す実装は配列要素内・ネストクラス内で壊れる。
    /// </summary>
    public static class PropertyPathUtility
    {
        private const string ARRAY_DATA_TOKEN = ".Array.data[";

        /// <summary>親プロパティのパスを返す。トップレベルなら空文字。</summary>
        public static string GetParentPath(string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath)) return string.Empty;

            // 配列要素そのもの（"xs.Array.data[3]"）の親はコンテナ（"xs"）
            if (propertyPath.EndsWith("]", StringComparison.Ordinal))
            {
                int token = propertyPath.LastIndexOf(ARRAY_DATA_TOKEN, StringComparison.Ordinal);
                if (token >= 0) return propertyPath.Substring(0, token);
            }

            int dot = propertyPath.LastIndexOf('.');
            return dot < 0 ? string.Empty : propertyPath.Substring(0, dot);
        }

        /// <summary>
        /// 同じ親を持つプロパティを引く。見つからなければ null。
        /// 段階2の [ShowIf] などで条件フィールドを参照するための入口。
        /// </summary>
        public static SerializedProperty FindSibling(SerializedProperty property, string siblingName)
        {
            string parentPath = GetParentPath(property.propertyPath);
            string path = parentPath.Length == 0 ? siblingName : $"{parentPath}.{siblingName}";
            return property.serializedObject.FindProperty(path);
        }

        /// <summary>
        /// rootPath を起点にした相対パスを、人が読める形で返す。
        /// "_items.Array.data[3].target" を rootPath "_items" で見ると "[3].target" になる。
        /// rootPath 自身なら空文字。
        /// </summary>
        public static string ToRelativePath(string rootPath, string propertyPath)
        {
            string readable = propertyPath.Replace(ARRAY_DATA_TOKEN, "[");
            string readableRoot = rootPath.Replace(ARRAY_DATA_TOKEN, "[");

            return readable.StartsWith(readableRoot, StringComparison.Ordinal)
                ? readable.Substring(readableRoot.Length)
                : readable;
        }
    }
}
