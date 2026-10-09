using System;
using System.Collections.Generic;
using NUnit.Framework;
using XXXL0C.Inspector.Editor;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class TypeCandidatesTest
    {
        public interface IShape
        {
        }

        [Serializable]
        public sealed class Circle : IShape
        {
        }

        public abstract class ShapeBase : IShape
        {
        }

        [Serializable]
        public sealed class Rect : ShapeBase
        {
        }

        // [Serializable] が無い
        public sealed class Plain : IShape
        {
        }

        [Serializable]
        public sealed class NeedsArgument : IShape
        {
            public NeedsArgument(int size)
            {
            }
        }

        [Test]
        public void SerializeReferenceの候補は生成できる具象クラスだけ()
        {
            List<Type> candidates = TypeCandidates.ForManagedReference(typeof(IShape), typeof(IShape));

            Assert.That(candidates, Is.EquivalentTo(new[] { typeof(Circle), typeof(Rect) }));
        }

        [Test]
        public void 基底型を指定するとさらに絞り込める()
        {
            List<Type> candidates = TypeCandidates.ForManagedReference(typeof(IShape), typeof(ShapeBase));

            Assert.That(candidates, Is.EquivalentTo(new[] { typeof(Rect) }));
        }

        [Test]
        public void SerializableTypeの候補は具象型ならよい()
        {
            IReadOnlyList<Type> candidates = TypeCandidates.ForSerializableType(typeof(IShape));

            Assert.That(candidates, Is.EquivalentTo(new[] { typeof(Circle), typeof(Rect), typeof(Plain), typeof(NeedsArgument) }));
            Assert.That(TypeCandidates.ContainsSerializableType(typeof(IShape), typeof(Plain).FullName), Is.True);
            Assert.That(TypeCandidates.ContainsSerializableType(typeof(IShape), "Removed.Shape"), Is.False);
        }

        [Test]
        public void 型名が重複するものだけ名前空間付きで表示する()
        {
            List<string> names = TypeCandidates.BuildDisplayNames(new[] { typeof(Circle), typeof(UnityEngine.Rect), typeof(Rect) });

            Assert.That(names[0], Is.EqualTo("Circle"));
            Assert.That(names[1], Is.EqualTo(typeof(UnityEngine.Rect).FullName));
            Assert.That(names[2], Is.EqualTo(typeof(Rect).FullName));
        }
    }
}
