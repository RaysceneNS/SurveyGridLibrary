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
            var markers = p.TownshipMarkers(1, 1, 1, 1);

            Assert.IsNotNull(markers);
            Assert.IsNotNull(markers.SouthEast);
            var southEast = markers.SouthEast.Value;

            Assert.AreEqual(49.0008010864258, southEast.Latitude, 0.000000001);
            Assert.AreEqual(97.4597702026367, southEast.Longitude, 0.000000001);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var p = new DlsTownshipMarkerProvider();
            var markers = p.TownshipMarkers(127, 127, 127, 127);

            Assert.IsNull(markers);
        }
    }
}
