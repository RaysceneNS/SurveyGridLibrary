using GisLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class DlsSectionMarkersTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var p = new DlsTownshipMarkerProvider();
            var list = p.TownshipMarkers(1, 1, 1);

            Assert.IsNotNull(list);

            Assert.AreEqual(36, list.Length);

            var secOne = list[0];

            Assert.IsNotNull(secOne.SouthEast);
            var southEast = secOne.SouthEast.Value;

            Assert.AreEqual(49.0008010864258, southEast.Latitude, 0.000000001);
            Assert.AreEqual(97.4597702026367, southEast.Longitude, 0.000000001);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var p = new DlsTownshipMarkerProvider();
            var list = p.TownshipMarkers(127, 127, 127);

            Assert.IsNull(list);
        }
    }
}
