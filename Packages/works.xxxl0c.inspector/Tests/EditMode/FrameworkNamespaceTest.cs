using NUnit.Framework;
using XXXL0C.Inspector.Editor;

namespace XXXL0C.Inspector.Tests.EditMode
{
    public sealed class FrameworkNamespaceTest
    {
        [TestCase("System")]
        [TestCase("System.Collections.Generic")]
        [TestCase("UnityEngine")]
        [TestCase("UnityEngine.UIElements")]
        [TestCase("UnityEditor")]
        [TestCase("Unity.Mathematics")]
        [TestCase("Microsoft.Win32")]
        public void フレームワークの名前空間を判定する(string namespaceName)
            => Assert.That(FrameworkNamespace.Contains(namespaceName), Is.True);

        [TestCase("UnityRoyale")]
        [TestCase("UnityRoyale.Battle")]
        [TestCase("SystemShock")]
        [TestCase("MicrosoftLike")]
        [TestCase("XXXL0C.Inspector")]
        [TestCase("")]
        [TestCase(null)]
        public void 名前が似ているだけの自作の名前空間は含めない(string namespaceName)
            => Assert.That(FrameworkNamespace.Contains(namespaceName), Is.False);
    }
}
