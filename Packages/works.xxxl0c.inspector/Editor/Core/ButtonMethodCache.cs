using System;
using System.Collections.Generic;
using System.Reflection;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 型が持つ [Button] 付きメソッドを列挙・キャッシュする。InspectedTypeCache のメソッド版。
    /// </summary>
    internal static class ButtonMethodCache
    {
        private const BindingFlags METHOD_FLAGS =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;

        private static readonly Dictionary<Type, IReadOnlyList<MethodInfo>> _cache =
            new Dictionary<Type, IReadOnlyList<MethodInfo>>();

        public static bool HasButtonMethods(Type type) => GetButtonMethods(type).Count > 0;

        /// <summary>
        /// [Button] 付きメソッドを宣言順・基底クラス方向に列挙する。
        /// 引数の有無は問わない（引数ありは構築側が「使えません」と通知する。ここで黙って除外しない）。
        /// </summary>
        public static IReadOnlyList<MethodInfo> GetButtonMethods(Type type)
        {
            if (type == null) return Array.Empty<MethodInfo>();
            if (_cache.TryGetValue(type, out IReadOnlyList<MethodInfo> cached)) return cached;

            List<MethodInfo> methods = new List<MethodInfo>();
            Type current = type;
            while (current != null && current != typeof(object) && !FrameworkNamespace.Contains(current.Namespace))
            {
                foreach (MethodInfo method in current.GetMethods(METHOD_FLAGS))
                {
                    bool hasButton = method.IsDefined(typeof(ButtonAttribute), false);
                    bool hasButtonGroup = method.IsDefined(typeof(ButtonGroupAttribute), false);
                    if ((hasButton || hasButtonGroup) && !method.IsGenericMethodDefinition)
                    {
                        methods.Add(method);
                    }
                }

                current = current.BaseType;
            }

            _cache[type] = methods;
            return methods;
        }
    }
}
