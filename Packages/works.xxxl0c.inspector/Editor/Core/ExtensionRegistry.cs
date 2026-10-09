using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// IPropertyDecorator / IValidationRule の実装を TypeCache で自動収集し、属性型から引けるようにする。
    /// 属性を足すときは「属性クラス + 実装クラス」だけで完結し、登録先を書き換える必要は無い。
    /// </summary>
    internal static class ExtensionRegistry
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";

        private static Dictionary<Type, IPropertyDecorator> _decorators;
        private static Dictionary<Type, IValidationRule> _rules;
        private static Dictionary<Type, IVisibilityRule> _visibilityRules;
        private static Dictionary<Type, IGroupContainerFactory> _groupFactories;
        private static Dictionary<Type, IFieldFactory> _fieldFactories;
        private static HashSet<Type> _handledAttributes;

        /// <summary>5系統の拡張点のいずれかが担当している属性型か。</summary>
        public static bool IsHandled(Type attributeType)
        {
            EnsureInitialized();
            return _handledAttributes.Contains(attributeType);
        }

        public static IPropertyDecorator FindDecorator(Type attributeType)
        {
            EnsureInitialized();
            return _decorators.TryGetValue(attributeType, out IPropertyDecorator decorator) ? decorator : null;
        }

        public static IValidationRule FindRule(Type attributeType)
        {
            EnsureInitialized();
            return _rules.TryGetValue(attributeType, out IValidationRule rule) ? rule : null;
        }

        public static IVisibilityRule FindVisibilityRule(Type attributeType)
        {
            EnsureInitialized();
            return _visibilityRules.TryGetValue(attributeType, out IVisibilityRule rule) ? rule : null;
        }

        public static IGroupContainerFactory FindGroupFactory(Type attributeType)
        {
            EnsureInitialized();
            return _groupFactories.TryGetValue(attributeType, out IGroupContainerFactory factory) ? factory : null;
        }

        public static IFieldFactory FindFieldFactory(Type attributeType)
        {
            EnsureInitialized();
            return _fieldFactories.TryGetValue(attributeType, out IFieldFactory factory) ? factory : null;
        }

        private static void EnsureInitialized()
        {
            // ドメインリロードで static が破棄されるので、実装を足したら自動的に作り直される
            if (_handledAttributes != null) return;

            _decorators = Collect<IPropertyDecorator>(decorator => decorator.AttributeType);
            _rules = Collect<IValidationRule>(rule => rule.AttributeType);
            _visibilityRules = Collect<IVisibilityRule>(rule => rule.AttributeType);
            _groupFactories = Collect<IGroupContainerFactory>(factory => factory.AttributeType);
            _fieldFactories = Collect<IFieldFactory>(factory => factory.AttributeType);

            _handledAttributes = new HashSet<Type>(_decorators.Keys);
            _handledAttributes.UnionWith(_rules.Keys);
            _handledAttributes.UnionWith(_visibilityRules.Keys);
            _handledAttributes.UnionWith(_groupFactories.Keys);
            _handledAttributes.UnionWith(_fieldFactories.Keys);
        }

        private static Dictionary<Type, T> Collect<T>(Func<T, Type> attributeTypeSelector) where T : class
        {
            Dictionary<Type, T> result = new Dictionary<Type, T>();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<T>())
            {
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition) continue;

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    Debug.LogError($"{LOG_PREFIX}{type.FullName} は引数なしコンストラクタを持たないため登録できません。");
                    continue;
                }

                T instance;
                try
                {
                    instance = (T)Activator.CreateInstance(type);
                }
                catch (Exception exception)
                {
                    // 1つの実装が壊れていてもインスペクタ全体を落とさない。ただし黙って捨てない
                    Debug.LogError($"{LOG_PREFIX}{type.FullName} の生成に失敗しました: {exception}");
                    continue;
                }

                Type attributeType = attributeTypeSelector(instance);
                if (attributeType == null)
                {
                    Debug.LogError($"{LOG_PREFIX}{type.FullName} の AttributeType が null です。");
                    continue;
                }

                if (result.TryGetValue(attributeType, out T existing))
                {
                    Debug.LogError(
                        $"{LOG_PREFIX}{attributeType.Name} を担当する実装が重複しています"
                        + $"（{existing.GetType().Name} / {type.Name}）。{existing.GetType().Name} を使用します。");
                    continue;
                }

                result.Add(attributeType, instance);
            }

            return result;
        }
    }
}
