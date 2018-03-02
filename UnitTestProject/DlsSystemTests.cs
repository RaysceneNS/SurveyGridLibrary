using System;
using GisLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class DlsSystemTests
    {
        [TestMethod]
        public void LoadPerformanceTest()
        {
            var start = DateTime.Now;

            for (byte meridian = 1; meridian <= 6; meridian++)
            {
                for (byte range = 1; range <= 32; range++)
                {
                    for (byte township = 1; township <= 127; township++)
                    {
                        var markers =  DlsSystem.TownshipMarkers(township, range, meridian);
                        Assert.IsNotNull(markers);
                        Assert.AreEqual(4, markers.Count);
                    }
                }
            }
            
            var end = DateTime.Now - start;
            Console.WriteLine("total time=" + end.TotalMilliseconds); 
        }

        [TestMethod]
        public void DirectionMoveTests()
        {
            var a = new DlsSystem(4, 11, 82, 4, 'W', 6);
            Assert.AreEqual(4, a.LegalSubdivision);
            Assert.AreEqual(11, a.Section);

            var b = a.GoEast();
            Assert.AreEqual(3, b.LegalSubdivision);
            Assert.AreEqual(11, b.Section);
            
            var c = a.GoWest();
            Assert.AreEqual(1, c.LegalSubdivision);
            Assert.AreEqual(10, c.Section);

            var d = a.GoNorth();
            Assert.AreEqual(5, d.LegalSubdivision);
            Assert.AreEqual(11, d.Section);

            var e = a.GoSouth();
            Assert.AreEqual(13, e.LegalSubdivision);
            Assert.AreEqual(2, e.Section);
        }

        [TestMethod]
        public void ToLatLongTests()
        {
            var a = new DlsSystem(4, 11, 82, 4, 'W', 6);
            var ll = a.ToLatLong();
            Assert.AreEqual(56.08892, ll.Latitude, 0.00001);
            Assert.AreEqual(-118.519378662109, ll.Longitude, 0.00001);

            ll = DlsSystem.ToLatLong(a);
            Assert.AreEqual(56.08892, ll.Latitude, 0.00001);
            Assert.AreEqual(-118.519378662109, ll.Longitude, 0.00001);
        }

        [TestMethod]
        public void InferCenterTests()
        {
            var ll = new LatLongCoordinate(56.08892f, -118.519378662109f);
            var a = DlsSystem.InferCenterLocation(ll);

            Assert.AreEqual('W', a.Direction);
            Assert.AreEqual(7, a.LegalSubdivision);
            Assert.AreEqual(6, a.Meridian);
            Assert.AreEqual("SE", a.Quarter);
            Assert.AreEqual(82, a.Township);
            Assert.AreEqual(4, a.Range);
            Assert.AreEqual(11, a.Section);
        }

        [TestMethod]
        public void CtorTests()
        {
            var a= new DlsSystem(4, 11, 82, 4, 'W', 6);
            Assert.AreEqual('W', a.Direction);
            Assert.AreEqual(4, a.LegalSubdivision);
            Assert.AreEqual(6, a.Meridian);
            Assert.AreEqual("SW", a.Quarter);
            Assert.AreEqual(82, a.Township);
            Assert.AreEqual(4, a.Range);
            Assert.AreEqual(11, a.Section);
        }

        [TestMethod]
        public void DirectionTests()
        {
            var a = DlsSystem.Parse("04-11-082-04W6");

            Assert.AreEqual('W', a.Direction);
            Assert.AreEqual(4, a.LegalSubdivision);
            Assert.AreEqual(6, a.Meridian);
            Assert.AreEqual("SW", a.Quarter);
            Assert.AreEqual(82, a.Township);
            Assert.AreEqual(4, a.Range);
            Assert.AreEqual(11, a.Section);
        }

        [TestMethod]
        public void ConversionTests()
        {
            var dls = new DlsSystem(7, 6, 5, 4, 'W', 5);

            var ll = dls.ToLatLong();
            Assert.AreEqual(49.354435, ll.Latitude, 0.000001);
            Assert.AreEqual(-114.524994, ll.Longitude, 0.000001);

            var bcnts = ll.ToBcNtsGridSystem();
            Assert.AreEqual("C-022-H/082-G-07", bcnts.ToString());
        }
    }
}
