using NUnit.Framework;
using XXXL0C.Inspector.Editor;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class PropertyPathUtilityTest
    {
        [TestCase("_value", "")]
        [TestCase("_nested._value", "_nested")]
        [TestCase("_items.Array.data[3]", "_items")]
        [TestCase("_items.Array.data[3]._value", "_items.Array.data[3]")]
        [TestCase("", "")]
        public void 親パスを返す(string path, string expected)
            => Assert.That(PropertyPathUtility.GetParentPath(path), Is.EqualTo(expected));

        [Test]
        public void 相対パスは配列添字を読みやすい形にする()
            => Assert.That(
                PropertyPathUtility.ToRelativePath("_items", "_items.Array.data[3].target"), Is.EqualTo("[3].target"));

        [Test]
        public void 起点そのものの相対パスは空文字()
            => Assert.That(PropertyPathUtility.ToRelativePath("_items", "_items"), Is.Empty);

        [Test]
        public void 起点の外のパスは読みやすくしてそのまま返す()
            => Assert.That(PropertyPathUtility.ToRelativePath("_other", "_items.Array.data[0]"), Is.EqualTo("_items[0]"));
    }
}
