using System.Collections.Generic;
using NUnit.Framework;
using XXXL0C.Inspector.Editor;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class SerializedFieldUtilityTest
    {
        [Test]
        public void フィールド名だけのパスは名前のステップに分かれる()
        {
            List<SerializedFieldUtility.PathStep> steps = SerializedFieldUtility.ParsePathSteps("_nested._value");

            Assert.That(steps.Count, Is.EqualTo(2));
            Assert.That(steps[0].Name, Is.EqualTo("_nested"));
            Assert.That(steps[0].Path, Is.EqualTo("_nested"));
            Assert.That(steps[1].Name, Is.EqualTo("_value"));
            Assert.That(steps[1].Path, Is.EqualTo("_nested._value"));
        }

        [Test]
        public void 配列要素は1つの添字ステップにまとまる()
        {
            List<SerializedFieldUtility.PathStep> steps =
                SerializedFieldUtility.ParsePathSteps("_items.Array.data[12]._value");

            Assert.That(steps.Count, Is.EqualTo(3));
            Assert.That(steps[1].IsIndex, Is.True);
            Assert.That(steps[1].Index, Is.EqualTo(12));
            Assert.That(steps[1].Path, Is.EqualTo("_items.Array.data[12]"));
            Assert.That(steps[2].Path, Is.EqualTo("_items.Array.data[12]._value"));
        }

        [Test]
        public void 入れ子の配列もそれぞれ添字ステップになる()
        {
            List<SerializedFieldUtility.PathStep> steps =
                SerializedFieldUtility.ParsePathSteps("_grid.Array.data[1]._row.Array.data[2]");

            Assert.That(steps.Count, Is.EqualTo(4));
            Assert.That(steps[1].Index, Is.EqualTo(1));
            Assert.That(steps[3].Index, Is.EqualTo(2));
        }

        [Test]
        public void Arrayという名前のフィールドは添字として扱わない()
        {
            List<SerializedFieldUtility.PathStep> steps = SerializedFieldUtility.ParsePathSteps("_holder.Array");

            Assert.That(steps.Count, Is.EqualTo(2));
            Assert.That(steps[1].IsIndex, Is.False);
            Assert.That(steps[1].Name, Is.EqualTo("Array"));
        }
    }
}
