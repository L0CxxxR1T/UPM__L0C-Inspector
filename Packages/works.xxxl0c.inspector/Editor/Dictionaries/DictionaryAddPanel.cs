using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// Dictionary の追加欄。キーを入力した時点で重複と null を弾くので、重複したまま要素が増えることは無い。
    /// 入力欄を用意できないキー型は既定値のキーで追加し、既定値のキーが既にあれば追加させない。
    /// </summary>
    internal sealed class DictionaryAddPanel : VisualElement
    {
        private const string LOG_PREFIX = "[XXXL0C.Inspector] ";
        private const string DEFAULT_KEY_NOTICE =
            "このキー型は入力欄に対応していないため、既定値のキーで追加します。追加後に表の中でキーを書き換えてください。";

        private readonly SerializedObject _serializedObject;
        private readonly string _propertyPath;
        private readonly Type _keyType;
        private readonly Type _valueType;
        private readonly DictionaryKeyInput _keyInput;
        private readonly Button _addButton;
        private readonly HelpBox _message;

        public DictionaryAddPanel(SerializedProperty property, Type keyType, Type valueType)
        {
            // SerializedProperty は保持せずパスから引き直す。Undo や再シリアライズで stale になるため
            _serializedObject = property.serializedObject;
            _propertyPath = property.propertyPath;
            _keyType = keyType;
            _valueType = valueType;

            AddToClassList(InspectorClassNames.DICTIONARY_ADD);

            VisualElement row = new VisualElement();
            row.AddToClassList(InspectorClassNames.DICTIONARY_ADD_ROW);
            Add(row);

            bool allowSceneObjects = !EditorUtility.IsPersistent(_serializedObject.targetObject);
            if (DictionaryKeyInput.TryCreate(keyType, allowSceneObjects, UpdateState, out _keyInput))
            {
                row.Add(_keyInput.Element);
            }
            else
            {
                Label notice = new Label(DEFAULT_KEY_NOTICE);
                notice.AddToClassList(InspectorClassNames.DICTIONARY_ADD_NOTICE);
                row.Add(notice);
            }

            _addButton = new Button(AddEntry) { text = "追加" };
            _addButton.AddToClassList(InspectorClassNames.DICTIONARY_ADD_BUTTON);
            row.Add(_addButton);

            _message = new HelpBox(string.Empty, HelpBoxMessageType.Error);
            _message.AddToClassList(InspectorClassNames.DICTIONARY_ADD_MESSAGE);
            _message.style.display = DisplayStyle.None;
            Add(_message);

            RegisterCallback<KeyDownEvent>(OnKeyDown);

            // 表の中でキーが書き換わると、入力中のキーの重複判定も変わる
            this.TrackPropertyValue(property, _ => UpdateState());
            UpdateState();
        }

        private void OnKeyDown(KeyDownEvent keyDownEvent)
        {
            if (keyDownEvent.keyCode != KeyCode.Return && keyDownEvent.keyCode != KeyCode.KeypadEnter) return;

            AddEntry();
            keyDownEvent.StopPropagation();
        }

        private void UpdateState()
        {
            bool canAdd = TryValidate(out _, out string error);
            _addButton.SetEnabled(canAdd);

            _message.text = error ?? string.Empty;
            _message.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        /// <param name="error">利用者に見せる理由。入力前の未指定など、見せるまでもない場合は null。</param>
        private bool TryValidate(out object key, out string error)
        {
            error = null;

            if (!TryGetCandidateKey(out key, out error)) return false;

            SerializedProperty dictionary = _serializedObject.FindProperty(_propertyPath);
            if (dictionary == null)
            {
                error = "Dictionary のプロパティが見つかりません。インスペクタを開き直してください。";
                return false;
            }

            int existing;
            try
            {
                existing = DictionaryUtility.IndexOfKey(dictionary, DictionaryUtility.ToComparableKey(key));
            }
            catch (Exception exception)
            {
                // キー型のユーザー実装（Equals）が投げた例外。追加は止め、原因は残す
                Debug.LogException(exception);
                error = $"キーの比較中に例外が発生しました: {exception.Message}";
                return false;
            }

            if (existing < 0) return true;

            SerializedProperty existingKey = dictionary.GetArrayElementAtIndex(existing)
                .FindPropertyRelative(DictionaryUtility.KEY_NAME);
            error = _keyInput == null
                ? "既定値のキーを持つ要素が既にあります。先にそのキーを書き換えてください。"
                : $"キー {DictionaryUtility.FormatKey(existingKey, existing)} は既に存在します。";
            return false;
        }

        private bool TryGetCandidateKey(out object key, out string error)
        {
            error = null;

            if (_keyInput == null)
            {
                key = _keyType.IsValueType
                    ? Activator.CreateInstance(_keyType)
                    : Activator.CreateInstance(_keyType, true);
                return true;
            }

            object raw = _keyInput.RawValue;

            // 未選択の ObjectField。入力前から赤くしないよう理由は出さない
            if (raw == null || (raw is UnityEngine.Object unityObject && unityObject == null))
            {
                key = null;
                return false;
            }

            if (_keyType.IsEnum || !_keyType.IsPrimitive || raw.GetType() == _keyType)
            {
                key = raw;
                return true;
            }

            try
            {
                key = Convert.ChangeType(raw, _keyType);
                return true;
            }
            catch (OverflowException)
            {
                key = null;
                error = $"キーが {_keyType.Name} の範囲外です。";
                return false;
            }
        }

        private void AddEntry()
        {
            if (!TryValidate(out object key, out _))
            {
                UpdateState();
                return;
            }

            SerializedProperty dictionary = _serializedObject.FindProperty(_propertyPath);
            int index = dictionary.arraySize;
            dictionary.arraySize = index + 1;

            SerializedProperty entry = dictionary.GetArrayElementAtIndex(index);
            DictionaryUtility.WriteKey(entry.FindPropertyRelative(DictionaryUtility.KEY_NAME), key);

            try
            {
                DictionaryUtility.ResetValue(entry.FindPropertyRelative(DictionaryUtility.VALUE_NAME), _valueType);
            }
            catch (Exception exception)
            {
                // 追加そのものは成立しているので止めない。値が直前の要素の複製になっていることは伝える
                Debug.LogError(
                    $"{LOG_PREFIX}{_valueType.Name} の既定値を作れなかったため、追加した要素の値は直前の要素の複製になっています: "
                    + exception.Message);
            }

            _serializedObject.ApplyModifiedProperties();

            _keyInput?.Reset();
            UpdateState();
        }
    }
}
