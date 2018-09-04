using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
{
    [TestClass]
    public class AngleTests
    {
        [TestMethod]
        public void TestFromRadians()
        {
            var angle = Angle.FromRadians((float)Math.PI*2);
            Assert.AreEqual(400, angle.Gradians);
            Assert.AreEqual(6283.18505859375, angle.Milliradians, 0.000000001);
            Assert.AreEqual(6.28318548202515, angle.Radians, 0.000000001);
            Assert.AreEqual(360, angle.Degrees);
            Assert.AreEqual(0, angle.Minutes);
            Assert.AreEqual(0, angle.Seconds);
        }

        [TestMethod]
        public void TestFromDegrees()
        {
            var angle = Angle.FromDegrees(360);
            Assert.AreEqual(400, angle.Gradians);
            Assert.AreEqual(6283.18505859375, angle.Milliradians, 0.000000001);
            Assert.AreEqual(6.28318548202515, angle.Radians, 0.000000001);
            Assert.AreEqual(0, angle.Minutes);
            Assert.AreEqual(0, angle.Seconds);
            Assert.AreEqual(360, angle.Degrees);
        }

        [TestMethod]
        public void TestEquality()
        {
            var a = Angle.FromRadians((float)Math.PI);
            var b = Angle.FromRadians((float)Math.PI);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));
        }

        [TestMethod]
        public void TestInequality()
        {
            var a = Angle.FromRadians((float)Math.PI);
            var b = Angle.FromRadians((float)Math.PI+0.25f);

            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [TestMethod]
        public void TestHashCode()
        {
            Assert.AreEqual(Angle.FromRadians((float)Math.PI).GetHashCode(), Angle.FromRadians((float)Math.PI).GetHashCode());
        }

        [TestMethod]
        public void TestToString()
        {
            var a = Angle.FromRadians((float)Math.PI);
            Assert.AreEqual("3.141593", a.ToString());
            Assert.AreEqual("3.141593", a.ToString("g"));
            Assert.AreEqual("180°", a.ToString("d"));
            Assert.AreEqual("180 00.000", a.ToString("DD MM.MMM"));
        }

        [TestMethod]
        public void TestComparison()
        {
            var a = Angle.FromDegrees(100);
            var b = Angle.FromDegrees(200);
            var c = Angle.FromDegrees(300);

            Assert.AreEqual(-1, a.CompareTo(b));
            Assert.AreEqual(0, b.CompareTo(b));
            Assert.AreEqual(1, c.CompareTo(b));
        }
    }
}
