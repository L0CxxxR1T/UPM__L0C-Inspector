using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Editor
{
    /// <summary>
    /// 追加欄でキーを入力するフィールド。キー型ごとに UI Toolkit の標準フィールドを割り当てる。
    /// 対応していない型（独自の構造体やクラス）は <see cref="TryCreate"/> が false を返す。
    /// </summary>
    internal sealed class DictionaryKeyInput
    {
        private const string LABEL = "追加するキー";

        private readonly Func<object> _getRaw;
        private readonly Action _reset;

        public VisualElement Element { get; }

        private DictionaryKeyInput(VisualElement element, Func<object> getRaw, Action reset)
        {
            Element = element;
            _getRaw = getRaw;
            _reset = reset;
        }

        /// <summary>入力中の値。フィールドの型のままなので、キー型への変換は呼び出し側が行う。</summary>
        public object RawValue => _getRaw();

        public void Reset() => _reset();

        public static bool TryCreate(Type keyType, bool allowSceneObjects, Action onChanged, out DictionaryKeyInput input)
        {
            input = Create(keyType, allowSceneObjects, onChanged);
            return input != null;
        }

        private static DictionaryKeyInput Create(Type keyType, bool allowSceneObjects, Action onChanged)
        {
            if (keyType == typeof(string)) return Wrap(new TextField(LABEL), string.Empty, onChanged);

            // 範囲の狭い整数型は IntegerField で受け、キー型への変換時に範囲外を検出する
            if (keyType == typeof(int) || keyType == typeof(short) || keyType == typeof(sbyte)
                || keyType == typeof(ushort) || keyType == typeof(byte))
            {
                return Wrap(new IntegerField(LABEL), 0, onChanged);
            }

            if (keyType == typeof(long) || keyType == typeof(uint)) return Wrap(new LongField(LABEL), 0L, onChanged);
            if (keyType == typeof(ulong)) return Wrap(new UnsignedLongField(LABEL), 0UL, onChanged);
            if (keyType == typeof(float)) return Wrap(new FloatField(LABEL), 0f, onChanged);
            if (keyType == typeof(double)) return Wrap(new DoubleField(LABEL), 0d, onChanged);
            if (keyType == typeof(bool)) return Wrap(new Toggle(LABEL), false, onChanged);
            if (keyType == typeof(Vector2)) return Wrap(new Vector2Field(LABEL), Vector2.zero, onChanged);
            if (keyType == typeof(Vector3)) return Wrap(new Vector3Field(LABEL), Vector3.zero, onChanged);
            if (keyType == typeof(Vector2Int)) return Wrap(new Vector2IntField(LABEL), Vector2Int.zero, onChanged);
            if (keyType == typeof(Vector3Int)) return Wrap(new Vector3IntField(LABEL), Vector3Int.zero, onChanged);
            if (keyType == typeof(Color)) return Wrap(new ColorField(LABEL), Color.white, onChanged);

            if (keyType.IsEnum)
            {
                Array values = Enum.GetValues(keyType);
                if (values.Length == 0) return null;

                Enum initial = (Enum)values.GetValue(0);
                BaseField<Enum> field = keyType.IsDefined(typeof(FlagsAttribute), false)
                    ? new EnumFlagsField(LABEL, initial)
                    : (BaseField<Enum>)new EnumField(LABEL, initial);
                return Wrap(field, initial, onChanged);
            }

            if (typeof(Object).IsAssignableFrom(keyType))
            {
                ObjectField field = new ObjectField(LABEL)
                {
                    objectType = keyType,
                    allowSceneObjects = allowSceneObjects
                };
                return Wrap(field, null, onChanged);
            }

            return null;
        }

        private static DictionaryKeyInput Wrap<T>(BaseField<T> field, T initial, Action onChanged)
        {
            field.value = initial;
            field.AddToClassList(InspectorClassNames.DICTIONARY_ADD_KEY);
            field.AddToClassList(BaseField<T>.alignedFieldUssClassName);
            field.RegisterValueChangedCallback(_ => onChanged());

            return new DictionaryKeyInput(field, () => field.value, () => field.value = initial);
        }
    }
}
