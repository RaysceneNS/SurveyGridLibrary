using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
{
    [TestClass]
    public class DlsSystemTests
    {
        [TestMethod]
        public void ToLatLongTests1()
        {
            var latLong = new DlsSystem(4, 11, 82, 4, 6).ToLatLong();
            Assert.AreEqual(56.08892, latLong.Latitude, 0.00001);
            Assert.AreEqual(-118.519378662109, latLong.Longitude, 0.00001);
        }

        [TestMethod]
        public void ToLatLongTests2()
        {
            var latLong = DlsSystem.ToLatLong(new DlsSystem(4, 11, 82, 4, 6));
            Assert.AreEqual(56.08892, latLong.Latitude, 0.00001);
            Assert.AreEqual(-118.519378662109, latLong.Longitude, 0.00001);
        }
        
        [TestMethod]
        public void CtorTests()
        {
            var dlsSystem = new DlsSystem(4, 11, 82, 4, 6);
            Assert.AreEqual('W', dlsSystem.Direction);
            Assert.AreEqual(4, dlsSystem.LegalSubdivision);
            Assert.AreEqual(6, dlsSystem.Meridian);
            Assert.AreEqual("SW", dlsSystem.Quarter);
            Assert.AreEqual(82, dlsSystem.Township);
            Assert.AreEqual(4, dlsSystem.Range);
            Assert.AreEqual(11, dlsSystem.Section);
        }

        [TestMethod]
        public void DirectionTests()
        {
            var dlsSystem = DlsSystem.Parse("04-11-082-04W6");
            Assert.AreEqual('W', dlsSystem.Direction);
            Assert.AreEqual(4, dlsSystem.LegalSubdivision);
            Assert.AreEqual(6, dlsSystem.Meridian);
            Assert.AreEqual("SW", dlsSystem.Quarter);
            Assert.AreEqual(82, dlsSystem.Township);
            Assert.AreEqual(4, dlsSystem.Range);
            Assert.AreEqual(11, dlsSystem.Section);
        }

        [TestMethod]
        public void ConversionTests()
        {
            var ll = new DlsSystem(7, 6, 5, 4, 5).ToLatLong();
            Assert.AreEqual(49.354435, ll.Latitude, 0.000001);
            Assert.AreEqual(-114.524994, ll.Longitude, 0.000001);

            var ntsGridSystem = ll.ToBcNtsGridSystem();
            Assert.AreEqual("C-022-H/082-G-07", ntsGridSystem.ToString());
        }

        [TestMethod]
        public void EnsureRelativeTests()
        {
            var dlsWest = new DlsSystem(8, 36, 23, 1, 5);
            var dlsEast = new DlsSystem(5, 33, 23, 29, 4);
            var dls932 = new DlsSystem(9, 36, 23, 1, 5);

            var latWest = dlsWest.ToLatLong();
            var latEast = dlsEast.ToLatLong();
            var lat932 = dls932.ToLatLong();
            Assert.IsTrue(latWest.Longitude < latEast.Longitude);
            
            var latTest = new LatLongCoordinate(51, -114);
            var a = latTest.RelativeDistanceTo(latWest);
            var b = latTest.RelativeDistanceTo(latEast);
            var c = latTest.RelativeDistanceTo(lat932);

            //assert that location b is closest to the test location
            Assert.IsTrue(b < a);
            Assert.IsTrue(b < c);
        }

        [TestMethod]
        public void TestEquality()
        {
            var a = new DlsSystem(8, 36, 23, 1, 5);
            var b = new DlsSystem(8, 36, 23, 1, 5);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));
        }

        [TestMethod]
        public void TestInequality()
        {
            var a = new DlsSystem(8, 36, 23, 1, 5);
            var b = new DlsSystem(8, 36, 23, 1, 4);

            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [TestMethod]
        public void TestHashCode()
        {
            Assert.AreEqual(new DlsSystem(8, 36, 23, 1, 5).GetHashCode(), new DlsSystem(8, 36, 23, 1, 5).GetHashCode());
        }
    }
}
