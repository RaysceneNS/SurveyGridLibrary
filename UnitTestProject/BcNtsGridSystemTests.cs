using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
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
        }

        [TestMethod]
        public void TestInequality()
        {
            var a = new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10);
            var b = new BcNtsGridSystem('A', 1, 'L', 93, 'F', 10);

            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [TestMethod]
        public void TestHashCode()
        {
            Assert.AreEqual(new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10).GetHashCode(),
                new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10).GetHashCode());
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
            Assert.AreEqual("B-001-L/093-F-10", new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10).ToString());
        }

        [TestMethod]
        public void TestParse()
        {
            var a = new BcNtsGridSystem('B', 1, 'L', 93, 'F', 10);
            var b = BcNtsGridSystem.Parse("B-001-L/093-F-10");
            Assert.IsTrue(a == b);
        }

        [TestMethod]
        public void TestToLatLong1()
        {
            var coordinate = BcNtsGridSystem.ToLatLong(new BcNtsGridSystem('A', 1, 'J', 93, 'P', 8));
            Assert.AreEqual(55.41875, coordinate.Latitude, 0.00001);
            Assert.AreEqual(-120.128128, coordinate.Longitude, 0.000001);
        }

        [TestMethod]
        public void TestToLatLong2()
        {
            var coordinate = new BcNtsGridSystem('A', 1, 'J', 93, 'P', 8).ToLatLong();
            Assert.AreEqual(55.41875, coordinate.Latitude, 0.00001);
            Assert.AreEqual(-120.128128, coordinate.Longitude, 0.000001);
        }
    }
}
