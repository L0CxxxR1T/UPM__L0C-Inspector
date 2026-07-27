using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 「その型にカスタム PropertyDrawer が当たっているか」を判定する。
    /// ネストしたクラスを自前展開してよいかの判断に使う（他人の Drawer の描画を奪わないため）。
    /// </summary>
    /// <remarks>
    /// CustomPropertyDrawer は対象の型を public に公開していないので、private フィールドを
    /// リフレクションで読む。ここがこのパッケージで唯一 Unity の内部構造に踏み込んでいる箇所。
    /// 読めなかった場合は HasDrawer が常に true を返して自前展開を止める。
    /// つまり最悪でも「PropertyField に丸投げ」に戻るだけで、誰の描画も壊さない。
    /// </remarks>
    internal static class PropertyDrawerRegistry
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";
        private const string TYPE_FIELD = "m_Type";
        private const string USE_FOR_CHILDREN_FIELD = "m_UseForChildren";

        private static HashSet<Type> _exactTypes;
        private static List<Type> _baseTypes;
        private static bool _degraded;

        public static bool HasDrawer(Type type)
        {
            EnsureInitialized();

            // 判定できないときは「Drawer あり」に倒して展開しない
            if (_degraded) return true;
            if (type == null) return false;

            if (_exactTypes.Contains(type)) return true;

            // [CustomPropertyDrawer(typeof(Foo<,>), true)] のようなオープンジェネリクス指定
            if (type.IsGenericType && _exactTypes.Contains(type.GetGenericTypeDefinition())) return true;

            foreach (Type baseType in _baseTypes)
            {
                if (baseType.IsGenericTypeDefinition)
                {
                    if (type.IsGenericType && baseType == type.GetGenericTypeDefinition()) return true;
                    continue;
                }

                if (baseType.IsAssignableFrom(type)) return true;
            }

            return false;
        }

        private static void EnsureInitialized()
        {
            if (_exactTypes != null) return;

            _exactTypes = new HashSet<Type>();
            _baseTypes = new List<Type>();

            FieldInfo typeField = typeof(CustomPropertyDrawer)
                .GetField(TYPE_FIELD, BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo useForChildrenField = typeof(CustomPropertyDrawer)
                .GetField(USE_FOR_CHILDREN_FIELD, BindingFlags.Instance | BindingFlags.NonPublic);

            if (typeField == null || useForChildrenField == null)
            {
                _degraded = true;
                Debug.LogWarning(
                    $"{LOG_PREFIX}CustomPropertyDrawer の内部構造を読めなかったため、"
                    + "ネストしたクラスの自前展開を無効にします。ネスト内の装飾属性は反映されません。");
                return;
            }

            foreach (Type drawerType in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
            {
                foreach (CustomPropertyDrawer attribute
                         in drawerType.GetCustomAttributes<CustomPropertyDrawer>(false))
                {
                    if (!(typeField.GetValue(attribute) is Type target)) continue;

                    _exactTypes.Add(target);

                    if (useForChildrenField.GetValue(attribute) is bool useForChildren && useForChildren)
                    {
                        _baseTypes.Add(target);
                    }
                }
            }
        }
    }
}
