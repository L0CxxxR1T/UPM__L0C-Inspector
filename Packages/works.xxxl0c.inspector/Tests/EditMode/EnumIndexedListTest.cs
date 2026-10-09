using System;
using NUnit.Framework;
using UnityEngine;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class EnumIndexedListTest
    {
        private enum Fruit
        {
            Apple,
            Banana,
            Cherry
        }

        private enum Flag
        {
            One = 1,
            Two = 2,
            Four = 4,
            Alias = 2
        }

        [Serializable]
        private sealed class Holder
        {
            [SerializeField] private EnumIndexedList<Fruit, int> _list = new EnumIndexedList<Fruit, int>();

            public EnumIndexedList<Fruit, int> List => _list;
        }

        [Test]
        public void 要素数はenumのメンバー数と同じ()
            => Assert.That(new EnumIndexedList<Fruit, int>().Count, Is.EqualTo(3));

        [Test]
        public void メンバーごとに独立して読み書きできる()
        {
            EnumIndexedList<Fruit, int> list = new EnumIndexedList<Fruit, int>();
            list[Fruit.Apple] = 1;
            list[Fruit.Banana] = 2;
            list[Fruit.Cherry] = 3;

            Assert.That(list[Fruit.Apple], Is.EqualTo(1));
            Assert.That(list[Fruit.Banana], Is.EqualTo(2));
            Assert.That(list[Fruit.Cherry], Is.EqualTo(3));
        }

        [Test]
        public void 書き込む前は既定値()
            => Assert.That(new EnumIndexedList<Fruit, string>()[Fruit.Cherry], Is.Null);

        [Test]
        public void 値が飛び飛びのenumでも使える()
        {
            EnumIndexedList<Flag, int> list = new EnumIndexedList<Flag, int>();
            list[Flag.Four] = 4;

            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[Flag.Four], Is.EqualTo(4));
        }

        [Test]
        public void 同じ値の別名は同じ要素を指す()
        {
            EnumIndexedList<Flag, int> list = new EnumIndexedList<Flag, int>();
            list[Flag.Alias] = 22;

            Assert.That(list[Flag.Two], Is.EqualTo(22));
        }

        [Test]
        public void 定義されていない値は例外になる()
            => Assert.Throws<ArgumentOutOfRangeException>(() => _ = new EnumIndexedList<Fruit, int>()[(Fruit)99]);

        [Test]
        public void 要素数が足りない古いデータは読み込み時に埋める()
        {
            Holder holder = JsonUtility.FromJson<Holder>("{\"_list\":{\"_items\":[5]}}");

            Assert.That(holder.List[Fruit.Apple], Is.EqualTo(5));
            Assert.That(holder.List[Fruit.Cherry], Is.EqualTo(0));
        }
    }
}
