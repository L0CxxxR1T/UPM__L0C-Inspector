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
    /// [Button] 付きメソッドをインスペクタのボタンとして組み立てる。
    /// フィールド前提の5拡張点（装飾・可視性・検証・グループ・本体差し替え）とは別軸の仕組みで、
    /// 属性が1つしかないので TypeCache 経由の拡張点は作らずここに直接書く。
    /// </summary>
    internal static class ButtonSectionBuilder
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";
        private const string TOOLTIP_EDITOR_ONLY = "Edit Mode 中のみ実行できます。";
        private const string TOOLTIP_PLAY_MODE_ONLY = "Play Mode 中のみ実行できます。";

        private static readonly FoldoutGroupFactory _defaultGroupFactory = new FoldoutGroupFactory();

        /// <summary>
        /// targetType の [Button] メソッドをすべて組み立てて配置する。
        /// グループ指定が無ければ rootNode（フッター）に、あればそのパスのグループに入れる。
        /// </summary>
        public static void Build(
            SerializedObject serializedObject, Type targetType, GroupNode rootNode, GroupTree groupTree)
        {
            Dictionary<string, VisualElement> buttonRows = new Dictionary<string, VisualElement>(StringComparer.Ordinal);
            foreach (MethodInfo method in ButtonMethodCache.GetButtonMethods(targetType))
            {
                ButtonAttribute attribute = method.GetCustomAttribute<ButtonAttribute>() ?? new ButtonAttribute();
                ButtonGroupAttribute buttonGroup = method.GetCustomAttribute<ButtonGroupAttribute>();

                GroupNode ownerNode = string.IsNullOrEmpty(attribute.Group)
                    ? rootNode
                    : groupTree.Resolve(rootNode, attribute.Group, _defaultGroupFactory, allowExistingTypeMismatch: true);

                ownerNode.MarkHasStaticContent();
                VisualElement parent = ownerNode.Content;
                if (buttonGroup != null)
                {
                    string groupKey = $"{ownerNode.Key}\0{buttonGroup.Name ?? string.Empty}";
                    if (!buttonRows.TryGetValue(groupKey, out VisualElement row))
                    {
                        row = new VisualElement();
                        row.AddToClassList(InspectorClassNames.BUTTON_ROW);
                        parent.Add(row);
                        buttonRows.Add(groupKey, row);
                    }

                    parent = row;
                }

                AddButton(parent, serializedObject, method, attribute);
            }
        }

        private static void AddButton(
            VisualElement parent, SerializedObject serializedObject, MethodInfo method, ButtonAttribute attribute)
        {
            ParameterInfo[] parameters = method.GetParameters();
            foreach (ParameterInfo parameter in parameters)
            {
                if (!IsSupportedParameter(parameter))
                {
                    HelpBox notice = new HelpBox(
                        $"[Button] は対応する型の通常引数にのみ使えます（{method.Name}）。", HelpBoxMessageType.Warning);
                    notice.AddToClassList(InspectorClassNames.NOTICE);
                    parent.Add(notice);
                    return;
                }
            }

            string label = string.IsNullOrEmpty(attribute.Label)
                ? ObjectNames.NicifyVariableName(method.Name)
                : attribute.Label;

            VisualElement block = new VisualElement();
            block.AddToClassList(InspectorClassNames.BUTTON_BLOCK);

            object[] arguments = new object[parameters.Length];
            if (parameters.Length > 0)
            {
                VisualElement parameterFields = new VisualElement();
                parameterFields.AddToClassList(InspectorClassNames.BUTTON_PARAMETERS);
                for (int i = 0; i < parameters.Length; i++)
                {
                    arguments[i] = GetDefaultValue(parameters[i]);
                    parameterFields.Add(CreateParameterField(parameters[i], arguments, i));
                }

                block.Add(parameterFields);
            }

            VisualElement result = method.ReturnType == typeof(void) ? null : new VisualElement();
            if (result != null) result.AddToClassList(InspectorClassNames.BUTTON_RESULT);

            Button button = new Button(() => Invoke(serializedObject, method, label, arguments, result))
            {
                text = label
            };
            button.AddToClassList(InspectorClassNames.BUTTON);

            if (attribute.Mode != ButtonMode.Always)
            {
                PlayModeTracking.Track(button, () => ApplyModeState(button, attribute.Mode));
            }

            block.Add(button);
            if (result != null) block.Add(result);
            parent.Add(block);
        }

        private static bool IsSupportedParameter(ParameterInfo parameter)
        {
            Type type = parameter.ParameterType;
            if (type.IsByRef || parameter.IsOut) return false;

            return type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double)
                || type == typeof(string) || type == typeof(bool) || type.IsEnum
                || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4)
                || type == typeof(Vector2Int) || type == typeof(Vector3Int) || type == typeof(Color)
                || typeof(Object).IsAssignableFrom(type);
        }

        private static object GetDefaultValue(ParameterInfo parameter)
        {
            Type type = parameter.ParameterType;
            object value = parameter.HasDefaultValue ? parameter.DefaultValue : null;
            if (value == DBNull.Value || value == Missing.Value) value = null;

            if (type.IsEnum)
            {
                if (value == null) return Enum.ToObject(type, 0);
                return value.GetType() == type ? value : Enum.ToObject(type, value);
            }

            if (value != null) return value;
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static VisualElement CreateParameterField(ParameterInfo parameter, object[] arguments, int index)
        {
            string label = parameter.Name;
            Type type = parameter.ParameterType;

            if (type == typeof(int))
            {
                IntegerField field = new IntegerField(label) { value = (int)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(long))
            {
                LongField field = new LongField(label) { value = (long)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(float))
            {
                FloatField field = new FloatField(label) { value = (float)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(double))
            {
                DoubleField field = new DoubleField(label) { value = (double)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(string))
            {
                TextField field = new TextField(label) { value = (string)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(bool))
            {
                Toggle field = new Toggle(label) { value = (bool)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type.IsEnum)
            {
                Enum enumValue = (Enum)arguments[index];
                if (type.IsDefined(typeof(FlagsAttribute), false))
                {
                    EnumFlagsField field = new EnumFlagsField(label, enumValue);
                    field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                    return field;
                }

                EnumField enumField = new EnumField(label, enumValue);
                enumField.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return enumField;
            }

            if (type == typeof(Vector2))
            {
                Vector2Field field = new Vector2Field(label) { value = (Vector2)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(Vector3))
            {
                Vector3Field field = new Vector3Field(label) { value = (Vector3)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(Vector4))
            {
                Vector4Field field = new Vector4Field(label) { value = (Vector4)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(Vector2Int))
            {
                Vector2IntField field = new Vector2IntField(label) { value = (Vector2Int)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(Vector3Int))
            {
                Vector3IntField field = new Vector3IntField(label) { value = (Vector3Int)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            if (type == typeof(Color))
            {
                ColorField field = new ColorField(label) { value = (Color)arguments[index] };
                field.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
                return field;
            }

            ObjectField objectField = new ObjectField(label)
            {
                objectType = type,
                allowSceneObjects = true,
                value = (Object)arguments[index]
            };
            objectField.RegisterValueChangedCallback(changeEvent => arguments[index] = changeEvent.newValue);
            return objectField;
        }

        private static void ApplyModeState(Button button, ButtonMode mode)
        {
            bool matches = mode == ButtonMode.PlayModeOnly
                ? EditorApplication.isPlaying
                : !EditorApplication.isPlaying;

            button.SetEnabled(matches);
            button.tooltip = matches
                ? string.Empty
                : mode == ButtonMode.PlayModeOnly ? TOOLTIP_PLAY_MODE_ONLY : TOOLTIP_EDITOR_ONLY;
        }

        /// <summary>
        /// Instance メソッドは選択中の全オブジェクトに呼び、static メソッドは1回だけ呼ぶ。
        /// </summary>
        private static void Invoke(
            SerializedObject serializedObject, MethodInfo method, string label, object[] arguments, VisualElement result)
        {
            bool isStatic = method.IsStatic;
            Object[] targets = isStatic ? new Object[] { null } : serializedObject.targetObjects;

            int undoGroup = -1;
            if (!isStatic)
            {
                Undo.IncrementCurrentGroup();
                undoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName($"Button: {label}");
            }

            result?.Clear();
            foreach (Object target in targets)
            {
                if (!isStatic) Undo.RecordObject(target, label);

                try
                {
                    object value = method.Invoke(isStatic ? null : target, arguments);
                    if (result != null)
                    {
                        string text = isStatic ? FormatResult(value) : $"{target.name}: {FormatResult(value)}";
                        Label resultLabel = new Label(text);
                        resultLabel.AddToClassList(InspectorClassNames.BUTTON_RESULT);
                        result.Add(resultLabel);
                    }
                }
                catch (TargetInvocationException exception)
                {
                    Exception cause = exception.InnerException ?? exception;
                    if (target == null) Debug.LogException(cause);
                    else Debug.LogException(cause, target);
                }

                if (!isStatic) EditorUtility.SetDirty(target);
            }

            if (!isStatic) Undo.CollapseUndoOperations(undoGroup);
        }

        private static string FormatResult(object value) => value == null ? "(null)" : value.ToString();
    }
}
