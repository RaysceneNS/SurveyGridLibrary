using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
{
    [TestClass]
    public class DlsMarkerProviderTests
    {
        [TestMethod]
        public void LoadPerformanceTest()
        {
            var start = DateTime.Now;
            var provider = DlsSurveyCoordinateProvider.Instance;
            Assert.IsNotNull(provider);
            var end = DateTime.Now - start;
            Console.WriteLine("provider load time=" + end.TotalMilliseconds);
        }

        [TestMethod]
        public void IterateAllTownships()
        {
            var provider = DlsSurveyCoordinateProvider.Instance;
            for (byte meridian = 1; meridian <= 6; meridian++)
            {
                for (byte range = 1; range <= 32; range++)
                {
                    for (byte township = 1; township <= 127; township++)
                    {
                        var markers = provider.TownshipBoundary(township, range, meridian);
                    }
                }
            }
        }

        [TestMethod]
        public void VerifyFirstMarker()
        {
            var markers = DlsSurveyCoordinateProvider.Instance.BoundaryMarkers(1, 1, 1, 1);

            Assert.IsNotNull(markers);
            Assert.IsNotNull(markers.SouthEast);
            var coordinate = markers.SouthEast;

            Assert.AreEqual(49.000801086426, coordinate.Value.Latitude, 0.0000001);
            Assert.AreEqual(-97.459770202637, coordinate.Value.Longitude, 0.0000001);
        }

        [TestMethod]
        public void VerifyLastMarker()
        {
            var markers = DlsSurveyCoordinateProvider.Instance.BoundaryMarkers(36, 78, 15, 6);

            Assert.IsNotNull(markers);
            Assert.IsNotNull(markers.NorthEast);
            var coordinate = markers.NorthEast;

            Assert.AreEqual(55.8103638, coordinate.Value.Latitude, 0.000001);
            Assert.AreEqual(-120.172646, coordinate.Value.Longitude, 0.000001);
        }

        [TestMethod]
        public void AssertMarkersMatchKnown()
        {
            var markers = DlsSurveyCoordinateProvider.Instance.BoundaryMarkers(6, 1, 30, 3);

            Assert.IsNotNull(markers.SouthEast);
            Assert.AreEqual(48.9997524, markers.SouthEast.Value.Latitude, 0.000001);
            Assert.AreEqual(-109.991165, markers.SouthEast.Value.Longitude, 0.000001);

            Assert.IsNotNull(markers.SouthWest);
            Assert.AreEqual(48.99978247, markers.SouthWest.Value.Latitude, 0.000001);
            Assert.AreEqual(-110.00480669, markers.SouthWest.Value.Longitude, 0.000001);

            Assert.IsNotNull(markers.NorthEast);
            Assert.AreEqual(49.014183, markers.NorthEast.Value.Latitude, 0.000001);
            Assert.AreEqual(-109.9911499, markers.NorthEast.Value.Longitude, 0.000001);

            Assert.IsNotNull(markers.NorthWest);
            Assert.AreEqual(49.0142335, markers.NorthWest.Value.Latitude, 0.000001);
            Assert.AreEqual(-110.00480651, markers.NorthWest.Value.Longitude, 0.000001);

            //known LSD corners for 3-6-1-30-W3
            //SW 49.003384° -110.00206°
            //NW 48.999776° -110.002064°
            //NE 48.999782° -110.004807°
            //SE 49.003394° -110.004808°
        }

        [TestMethod]
        public void TestInvalidSectionMarker()
        {
            var markers = DlsSurveyCoordinateProvider.Instance.BoundaryMarkers(127, 127, 127, 127);
            Assert.IsNull(markers);
        }
        
        [TestMethod]
        public void TestKnownTownshipMarkers()
        {
            var markers = DlsSurveyCoordinateProvider.Instance.TownshipBoundary(23, 29, 4);
            // this township has only 85 markers as it is cut in half by Meridian 5.
            Assert.IsNotNull(markers);
        }
    }
}
