using NUnit.Framework;
using XXXL0C.Inspector.Editor;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class SteppedValueUtilityTest
    {
        private const float TOLERANCE = 1e-5f;

        [TestCase(0.24f, 0f)]
        [TestCase(0.26f, 0.5f)]
        [TestCase(7.6f, 7.5f)]
        public void 最小値を起点にステップへ丸める(float value, float expected)
            => Assert.That(SteppedValueUtility.Snap(value, 0f, 10f, 0.5f), Is.EqualTo(expected).Within(TOLERANCE));

        [Test]
        public void 最小値が0でなくても最小値を起点に丸める()
            => Assert.That(SteppedValueUtility.Snap(2.4f, 1f, 10f, 2f), Is.EqualTo(3f).Within(TOLERANCE));

        [TestCase(-5f, 0f)]
        [TestCase(15f, 10f)]
        public void 範囲外は範囲内に収める(float value, float expected)
            => Assert.That(SteppedValueUtility.Snap(value, 0f, 10f, 0.5f), Is.EqualTo(expected));

        [Test]
        public void 丸めた結果が最大値を超える場合は最大値に収める()
            => Assert.That(SteppedValueUtility.Snap(9.9f, 0f, 9.9f, 2f), Is.EqualTo(9.9f).Within(TOLERANCE));

        [Test]
        public void ステップが0以下なら丸めず範囲だけ収める()
            => Assert.That(SteppedValueUtility.Snap(3.33f, 0f, 10f, 0f), Is.EqualTo(3.33f).Within(TOLERANCE));
    }
}
