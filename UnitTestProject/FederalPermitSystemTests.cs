using GisLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
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

            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        [TestMethod]
        public void TestProperties()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);
            Assert.AreEqual('L', a.Unit);
            Assert.AreEqual(55, a.Section);
            Assert.AreEqual(70, a.LatDegrees);
            Assert.AreEqual(30, a.LatMinutes);
            Assert.AreEqual(-136, a.LonDegrees);
            Assert.AreEqual(0, a.LonMinutes);
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

            var str = a.ToString();

            var b = FederalPermitSystem.Parse(str);

            Assert.IsTrue(a == b);
        }

        [TestMethod]
        public void TestToLatLong()
        {
            var a = new FederalPermitSystem('L', 55, 70, 30, 136, 00);

            var c = FederalPermitSystem.ToLatLong(a);
            Assert.AreEqual(70.5749969482422, c.Latitude, 0.00001);
            Assert.AreEqual(-136.287506103516, c.Longitude, 0.000001);

            var d = a.ToLatLong();
            Assert.AreEqual(70.5749969482422, d.Latitude, 0.00001);
            Assert.AreEqual(-136.287506103516, d.Longitude, 0.000001);
        }
    }
}
