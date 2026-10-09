using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// [TypeFilter] のフィールドファクトリ。[SerializeReference] フィールドを、型を選ぶドロップダウンと
    /// 中身のフィールドで表示する。Unity 標準には [SerializeReference] の型を選ぶ UI が無いため。
    /// SerializableType（とその配列・List）は型の Drawer が [TypeFilter] を読んで描くので、PropertyField に任せる。
    /// </summary>
    public sealed class TypeFilterFieldFactory : IFieldFactory
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";

        public Type AttributeType => typeof(TypeFilterAttribute);

        public FieldFactoryResult Create(FieldFactoryContext context)
        {
            SerializedProperty property = context.Property;
            Type fieldType = context.FieldInfo.FieldType;

            Type elementType = SerializedFieldUtility.GetCollectionElementType(fieldType) ?? fieldType;
            if (elementType == typeof(SerializableType))
            {
                PropertyField field = new PropertyField(property);
                return FieldFactoryResult.Accept(field, null);
            }

            bool isCollection = elementType != fieldType;
            if (!isCollection && property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return FieldFactoryResult.Decline(
                    "[TypeFilter] は [SerializeReference] を付けたフィールドか、SerializableType にのみ使えます。");
            }

            TypeFilterAttribute attribute = (TypeFilterAttribute)context.Attribute;
            Type baseType = attribute.BaseType ?? elementType;

            List<Type> candidates = TypeCandidates.ForManagedReference(elementType, baseType);
            if (candidates.Count == 0)
            {
                return FieldFactoryResult.Decline(
                    $"[TypeFilter] で選べる型がありません。{baseType.Name} を継承した [Serializable] な具象クラスを用意してください。");
            }

            List<string> choices = new List<string> { TypeCandidates.NONE_CHOICE };
            choices.AddRange(TypeCandidates.BuildDisplayNames(candidates));

            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            if (isCollection)
            {
                return FieldFactoryResult.Accept(
                    CreateManagedReferenceList(property, propertyPath, serializedObject, candidates, choices), null);
            }

            VisualElement root = CreateManagedReferenceField(
                property, propertyPath, serializedObject, candidates, choices, property.displayName);
            DropdownField dropdown = root.Q<DropdownField>();
            return FieldFactoryResult.Accept(root, dropdown.labelElement);
        }

        private static ListView CreateManagedReferenceList(
            SerializedProperty property,
            string propertyPath,
            SerializedObject serializedObject,
            List<Type> candidates,
            List<string> choices)
        {
            ListView listView = new ListView
            {
                showFoldoutHeader = true,
                showAddRemoveFooter = true,
                reorderable = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                headerTitle = property.displayName,
                makeItem = () => new VisualElement()
            };
            listView.AddToClassList(InspectorClassNames.TYPE_FILTER_LIST);

            listView.bindItem = (element, index) =>
            {
                element.Clear();
                SerializedProperty array = serializedObject.FindProperty(propertyPath);
                if (array == null || index >= array.arraySize) return;

                SerializedProperty item = array.GetArrayElementAtIndex(index);
                if (item.propertyType != SerializedPropertyType.ManagedReference) return;

                string itemPath = item.propertyPath;
                element.Add(CreateManagedReferenceField(
                    item, itemPath, serializedObject, candidates, choices, string.Empty));
            };

            listView.onAdd = sourceListView =>
            {
                SerializedProperty array = serializedObject.FindProperty(propertyPath);
                if (array == null) return;

                int index = array.arraySize;
                array.arraySize++;
                array.GetArrayElementAtIndex(index).managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                sourceListView.RefreshItems();
            };

            listView.BindProperty(property);
            return listView;
        }

        private static VisualElement CreateManagedReferenceField(
            SerializedProperty property,
            string propertyPath,
            SerializedObject serializedObject,
            List<Type> candidates,
            List<string> choices,
            string label)
        {
            VisualElement root = new VisualElement();
            DropdownField dropdown = new DropdownField(label, choices, 0);
            if (!string.IsNullOrEmpty(label)) dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            VisualElement content = new VisualElement();
            content.AddToClassList(InspectorClassNames.TYPE_FILTER_CONTENT);
            root.Add(dropdown);
            root.Add(content);

            string shownTypename = null;

            void Sync(SerializedProperty current)
            {
                // 中身の編集でも呼ばれる。型が変わったときだけ作り直す（入力中のフォーカスを奪わない）
                if (string.Equals(shownTypename, current.managedReferenceFullTypename, StringComparison.Ordinal)) return;

                shownTypename = current.managedReferenceFullTypename;
                Type currentType = SerializedFieldUtility.ResolveManagedReferenceType(current);
                int index = currentType == null ? -1 : candidates.IndexOf(currentType);
                dropdown.SetValueWithoutNotify(
                    index >= 0 ? choices[index + 1] : currentType == null ? TypeCandidates.NONE_CHOICE : currentType.Name);

                RebuildContent(content, current, currentType, serializedObject);
            }

            dropdown.RegisterValueChangedCallback(changeEvent =>
            {
                int index = choices.IndexOf(changeEvent.newValue);
                Type selected = index <= 0 ? null : candidates[index - 1];
                Assign(serializedObject, propertyPath, selected);

                SerializedProperty current = serializedObject.FindProperty(propertyPath);
                if (current != null) Sync(current);
            });

            root.TrackPropertyValue(property, Sync);
            Sync(property);
            return root;
        }

        /// <summary>選択中の全オブジェクトに、それぞれ新しいインスタンスを入れる（同じインスタンスを共有させない）。</summary>
        private static void Assign(SerializedObject serializedObject, string propertyPath, Type selected)
        {
            foreach (Object target in serializedObject.targetObjects)
            {
                object instance;
                try
                {
                    instance = selected == null ? null : CreateInstance(selected);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"{LOG_PREFIX}{selected.FullName} を生成できませんでした: {exception}", target);
                    continue;
                }

                using (SerializedObject single = new SerializedObject(target))
                {
                    SerializedProperty property = single.FindProperty(propertyPath);
                    if (property == null) continue;

                    property.managedReferenceValue = instance;
                    single.ApplyModifiedProperties();
                }
            }

            serializedObject.Update();
        }

        private static object CreateInstance(Type type)
            => type.IsValueType ? Activator.CreateInstance(type) : Activator.CreateInstance(type, nonPublic: true);

        private static void RebuildContent(
            VisualElement content, SerializedProperty property, Type valueType, SerializedObject serializedObject)
        {
            content.Clear();
            if (valueType == null) return;

            foreach (SerializedProperty child in SerializedFieldUtility.EnumerateDirectChildren(property))
            {
                FieldInfo field = SerializedFieldUtility.FindSerializedField(valueType, child.name);
                if (field != null && field.IsDefined(typeof(HideInInspector), false)) continue;

                content.Add(new PropertyField(child));
            }

            content.Bind(serializedObject);
        }
    }
}
