using System;
using NUnit.Framework;
using UnityEngine;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class SerializableTypeTest
    {
        [Serializable]
        private sealed class Holder
        {
            [SerializeField] private SerializableType _type;

            public SerializableType Type
            {
                get => _type;
                set => _type = value;
            }
        }

        [SetUp]
        public void SetUp() => SerializableTypeRegistry.Clear();

        [TearDown]
        public void TearDown() => SerializableTypeRegistry.Clear();

        [Test]
        public void 型の完全名を保存する()
            => Assert.That(new SerializableType(typeof(Vector3)).TypeName, Is.EqualTo("UnityEngine.Vector3"));

        [Test]
        public void 既定値は未設定()
        {
            SerializableType empty = default;

            Assert.That(empty.IsEmpty, Is.True);
            Assert.That(empty.TypeName, Is.Empty);
            Assert.That(new SerializableType(null).IsEmpty, Is.True);
        }

        [Test]
        public void 登録した型だけ解決できる()
        {
            SerializableType vector = new SerializableType(typeof(Vector3));

            Assert.That(vector.TryResolve(out _), Is.False);

            SerializableTypeRegistry.Register<Vector3>();

            Assert.That(vector.TryResolve(out Type resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(typeof(Vector3)));
        }

        [Test]
        public void 未登録の型をResolveすると例外になる()
            => Assert.Throws<InvalidOperationException>(() => new SerializableType(typeof(Vector3)).Resolve());

        [Test]
        public void 未設定をResolveすると例外になる()
            => Assert.Throws<InvalidOperationException>(() => default(SerializableType).Resolve());

        [Test]
        public void 同じ型の再登録は何もしない()
        {
            SerializableTypeRegistry.Register<Vector3>();

            Assert.DoesNotThrow(() => SerializableTypeRegistry.Register<Vector3>());
        }

        [Test]
        public void JsonUtilityで往復しても型名が残る()
        {
            Holder holder = new Holder { Type = new SerializableType(typeof(Vector3)) };

            Holder restored = JsonUtility.FromJson<Holder>(JsonUtility.ToJson(holder));

            Assert.That(restored.Type, Is.EqualTo(holder.Type));
        }
    }
}
