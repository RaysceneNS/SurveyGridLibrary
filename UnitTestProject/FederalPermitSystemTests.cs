using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
{
    [TestClass]
    public class FederalPermitSystemTests
    {
        [TestMethod]
        public void TestEquality()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            var b = new FederalPermitSystem('L', 55, 70, 30, 136, 00);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));
        }

        [TestMethod]
        public void TestInequality()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            var b = new FederalPermitSystem('A', 55, 70, 30, 136, 00);

            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [TestMethod]
        public void TestHashCode()
        {
            Assert.AreEqual(new FederalPermitSystem('L', 55, 70, 30, 136, 00).GetHashCode(), new FederalPermitSystem('L', 55, 70, 30, 136, 00).GetHashCode());
        }

        [TestMethod]
        public void TestProperties()
        {
            var federalPermitSystem = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            Assert.AreEqual('L', federalPermitSystem.Unit);
            Assert.AreEqual(55, federalPermitSystem.Section);
            Assert.AreEqual(70, federalPermitSystem.LatDegrees);
            Assert.AreEqual(30, federalPermitSystem.LatMinutes);
            Assert.AreEqual(-136, federalPermitSystem.LonDegrees);
            Assert.AreEqual(0, federalPermitSystem.LonMinutes);
        }

        [TestMethod]
        public void TestToString()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            Assert.AreEqual("L-55-7030-13600", a.ToString());
            a = new FederalPermitSystem('L', 00, 70, 30, 136, 00);
            Assert.AreEqual("L-00-7030-13600", a.ToString());
            a = new FederalPermitSystem('L', 00, 70, 30, 99, 00);
            Assert.AreEqual("L-00-7030-09900", a.ToString());
        }

        [TestMethod]
        public void TestParse()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            var b = FederalPermitSystem.Parse("L-55-7030-13600");
            Assert.IsTrue(a == b);
        }

        [TestMethod]
        public void TestToLatLong1()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            var latLong = FederalPermitSystem.ToLatLong(a);
            Assert.AreEqual(70.5749969482422, latLong.Latitude, 0.00001);
            Assert.AreEqual(-136.287506103516, latLong.Longitude, 0.000001);
        }

        [TestMethod]
        public void TestToLatLong2()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            var latLong = a.ToLatLong();
            Assert.AreEqual(70.5749969482422, latLong.Latitude, 0.00001);
            Assert.AreEqual(-136.287506103516, latLong.Longitude, 0.000001);
        }
    }
}
