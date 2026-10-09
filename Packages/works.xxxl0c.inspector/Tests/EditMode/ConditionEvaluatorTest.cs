using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using XXXL0C.Inspector.Editor;
using Object = UnityEngine.Object;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class ConditionEvaluatorTest
    {
        private ConditionTestAsset _asset;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<ConditionTestAsset>();
            _serializedObject = new SerializedObject(_asset);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            Object.DestroyImmediate(_asset);
        }

        [Test]
        public void 期待値なしならboolの値をそのまま返す()
        {
            Set("_flag", property => property.boolValue = true);

            Assert.That(Evaluate("_count", "_flag", null, out bool matched, out _), Is.True);
            Assert.That(matched, Is.True);
        }

        [Test]
        public void 期待値なしでbool以外を指定すると評価できない()
        {
            Assert.That(Evaluate("_flag", "_count", null, out _, out string reason), Is.False);
            Assert.That(reason, Does.Contain("_count"));
        }

        [Test]
        public void enumは期待値のenumと比較できる()
        {
            Set("_kind", property => property.intValue = (int)ConditionTestKind.Second);

            Evaluate("_flag", "_kind", ConditionTestKind.Second, out bool second, out _);
            Evaluate("_flag", "_kind", ConditionTestKind.First, out bool first, out _);

            Assert.That(second, Is.True);
            Assert.That(first, Is.False);
        }

        [Test]
        public void intとfloatは数値として比較できる()
        {
            Set("_count", property => property.intValue = 3);
            Set("_ratio", property => property.floatValue = 0.5f);

            Evaluate("_flag", "_count", 3, out bool countMatched, out _);
            Evaluate("_flag", "_ratio", 0.5f, out bool ratioMatched, out _);

            Assert.That(countMatched, Is.True);
            Assert.That(ratioMatched, Is.True);
        }

        [Test]
        public void stringは大文字小文字を区別して比較する()
        {
            Set("_label", property => property.stringValue = "Boss");

            Evaluate("_flag", "_label", "Boss", out bool exact, out _);
            Evaluate("_flag", "_label", "boss", out bool differentCase, out _);

            Assert.That(exact, Is.True);
            Assert.That(differentCase, Is.False);
        }

        [Test]
        public void Object参照は設定済みかどうかをboolで比較できる()
        {
            Evaluate("_flag", "_material", false, out bool matched, out _);

            Assert.That(matched, Is.True);
        }

        [Test]
        public void 型が合わない期待値は評価できない()
        {
            Assert.That(Evaluate("_flag", "_label", 1, out _, out string reason), Is.False);
            Assert.That(reason, Is.Not.Empty);
        }

        [Test]
        public void 存在しないフィールドは評価できない()
        {
            Assert.That(Evaluate("_flag", "_missing", true, out _, out string reason), Is.False);
            Assert.That(reason, Does.Contain("_missing"));
        }

        [Test]
        public void ネストしたクラスの中では兄弟フィールドを相対で引く()
        {
            Set("_nested._enabled", property => property.boolValue = true);

            Assert.That(Evaluate("_nested._value", "_enabled", null, out bool matched, out _), Is.True);
            Assert.That(matched, Is.True);
        }

        [Test]
        public void 配列要素の中では同じ要素の兄弟フィールドを引く()
        {
            Set("_items", property => property.arraySize = 2);
            Set("_items.Array.data[1]._enabled", property => property.boolValue = true);

            Evaluate("_items.Array.data[0]._value", "_enabled", null, out bool first, out _);
            Evaluate("_items.Array.data[1]._value", "_enabled", null, out bool second, out _);

            Assert.That(first, Is.False);
            Assert.That(second, Is.True);
        }

        private void Set(string path, Action<SerializedProperty> write)
        {
            write(_serializedObject.FindProperty(path));
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private bool Evaluate(string path, string fieldName, object expected, out bool matched, out string reason)
            => ConditionEvaluator.TryEvaluate(
                _serializedObject.FindProperty(path), fieldName, expected, out matched, out reason);
    }
}
