using System;
using GisLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class AngleTests
    {
        [TestMethod]
        public void TestProperties()
        {
            var a = Angle.FromRadians((float)Math.PI*2);
            Assert.AreEqual(400, a.Gradians);
            Assert.AreEqual(6283.18505859375, a.Milliradians, 0.000000001);
            Assert.AreEqual(6.28318548202515, a.Radians, 0.000000001);
            Assert.AreEqual(360, a.Degrees);
            Assert.AreEqual(0, a.Minutes);
            Assert.AreEqual(0, a.Seconds);

            var b = Angle.FromDegrees(360);
            Assert.AreEqual(400, b.Gradians);
            Assert.AreEqual(6283.18505859375, b.Milliradians, 0.000000001);
            Assert.AreEqual(6.28318548202515, b.Radians, 0.000000001);
            Assert.AreEqual(0, b.Minutes);
            Assert.AreEqual(0, b.Seconds);
            Assert.AreEqual(360, b.Degrees);
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

            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
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
