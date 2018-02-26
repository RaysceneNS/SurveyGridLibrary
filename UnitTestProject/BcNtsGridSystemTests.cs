using GisLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class BcNtsGridSystemTests
    {
        [TestMethod]
        public void TestEquality()
        {
            var a = new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10);
            var b = new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));

            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        [TestMethod]
        public void TestProperties()
        {
            var a = new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10);
            Assert.AreEqual('L', a.Block);
            Assert.AreEqual('F', a.MapArea);
            Assert.AreEqual('B', a.QuarterUnit);
            Assert.AreEqual(93, a.Series);
            Assert.AreEqual(10, a.Sheet);
            Assert.AreEqual(1, a.Unit);
        }

        [TestMethod]
        public void TestToString()
        {
            var a = new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10);
            Assert.AreEqual("B-001-L/093-F-10", a.ToString());
        }

        [TestMethod]
        public void TestParse()
        {
            var a = new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10);

            var str = a.ToString();

            var b = BcNtsGridSystem.Parse(str);

            Assert.IsTrue(a==b);
        }

        [TestMethod]
        public void TestToLatLong()
        {
            var a = new BcNtsGridSystem('A', 1, 'J', 93, 'P', 8);

            var c = BcNtsGridSystem.ToLatLong(a);
            Assert.AreEqual(55.41875, c.Latitude, 0.00001);
            Assert.AreEqual(-120.128128, c.Longitude, 0.000001);

            var d = a.ToLatLong();
            Assert.AreEqual(55.41875, d.Latitude, 0.00001);
            Assert.AreEqual(-120.128128, d.Longitude, 0.000001);
        }
    }
}
