using System;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// リフレクションで降りても意味の無いフレームワーク側の名前空間か判定する。
    /// 前方一致ではなく区切り単位で比べる（"UnityRoyale" のような自作の名前空間を巻き込まない）。
    /// </summary>
    internal static class FrameworkNamespace
    {
        private static readonly string[] ROOTS = { "System", "Microsoft", "Unity", "UnityEngine", "UnityEditor" };

        public static bool Contains(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName)) return false;

            foreach (string root in ROOTS)
            {
                if (!namespaceName.StartsWith(root, StringComparison.Ordinal)) continue;
                if (namespaceName.Length == root.Length || namespaceName[root.Length] == '.') return true;
            }

            return false;
        }
    }
}
