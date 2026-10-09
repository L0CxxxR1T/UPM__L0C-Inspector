using System;
using NUnit.Framework;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class OptionalTest
    {
        [Test]
        public void Someは値を持つ()
        {
            Optional<int> optional = Optional<int>.Some(5);

            Assert.That(optional.HasValue, Is.True);
            Assert.That(optional.Value, Is.EqualTo(5));
        }

        [Test]
        public void Noneと既定値は値を持たない()
        {
            Assert.That(Optional<int>.None().HasValue, Is.False);
            Assert.That(default(Optional<int>).HasValue, Is.False);
        }

        [Test]
        public void 値が無いときにValueを読むと例外になる()
            => Assert.Throws<InvalidOperationException>(() => _ = Optional<int>.None().Value);

        [Test]
        public void 値が無いときは代わりの値を返す()
        {
            Assert.That(Optional<int>.Some(7).GetValueOrDefault(99), Is.EqualTo(7));
            Assert.That(Optional<int>.None().GetValueOrDefault(99), Is.EqualTo(99));
        }

        [Test]
        public void TryGetValueは値の有無を返す()
        {
            Assert.That(Optional<string>.Some("a").TryGetValue(out string some), Is.True);
            Assert.That(some, Is.EqualTo("a"));

            Assert.That(Optional<string>.None().TryGetValue(out string none), Is.False);
            Assert.That(none, Is.Null);
        }

        [Test]
        public void 値としてnullを持てる()
        {
            Optional<string> optional = Optional<string>.Some(null);

            Assert.That(optional.HasValue, Is.True);
            Assert.That(optional.Value, Is.Null);
        }
    }
}
