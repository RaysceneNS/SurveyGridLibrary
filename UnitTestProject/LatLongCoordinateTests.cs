using System;
using GisLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class LatLongCoordinateTests
    {
        [TestMethod]
        public void TestCtors()
        {
            //Degrees/Minute/Seconds
            var a = new LatLongCoordinate(50, 3, 4, 100, 1, 2);
            Assert.AreEqual(50.0511093139648, a.Latitude, 0.00000001);
            Assert.AreEqual(100.017219543457, a.Longitude, 0.00000001);

            //Degrees/Minutes
            var b = new LatLongCoordinate(50, 3, 100, 1);
            Assert.AreEqual(50.0499992370605, b.Latitude, 0.00000001);
            Assert.AreEqual(100.016670227051, b.Longitude, 0.00000001);
        }


        [TestMethod]
        public void TestFromRadians()
        {
            var ll = LatLongCoordinate.FromRadians((float) Math.PI, (float) Math.PI/2);

            Assert.AreEqual(90, ll.Latitude);
            Assert.AreEqual(90, ll.Longitude);

            Assert.AreEqual(Math.PI / 2, ll.LatitudeInRadians, 0.000001);
            Assert.AreEqual(Math.PI / 2, ll.LongitudeInRadians, 0.000001);

            Assert.AreEqual(Math.PI / 2, ll.RadiansLat, 0.000001);
            Assert.AreEqual(Math.PI / 2, ll.RadiansLon, 0.000001);
        }

        [TestMethod]
        public void TestClone()
        {
            var ll = LatLongCoordinate.FromRadians((float)Math.PI, (float)Math.PI / 2);
            var cloned = ll.Clone();

            Assert.AreNotSame(ll, cloned);
        }

        [TestMethod]
        public void TestEquality()
        {
            var a = new LatLongCoordinate(1, 10);
            var b = new LatLongCoordinate(1, 10);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));

            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        [TestMethod]
        public void TestToString()
        {
            var a = new LatLongCoordinate(1, 10);
            Assert.AreEqual("N1 00.000 E10 00.000", a.ToString());
            Assert.AreEqual("N1 00.000 E10 00.000", a.ToString("g"));
            Assert.AreEqual("1, 10", a.ToConvertibleString());
            Assert.AreEqual("1° 0' 0\" 10° 0' 0\"", a.ToDegreesMinutesSeconds());

            var b = new LatLongCoordinate(-1, 10);
            Assert.AreEqual("S1 00.000 E10 00.000", b.ToString());
            Assert.AreEqual("S1 00.000 E10 00.000", b.ToString("g"));
            Assert.AreEqual("-1, 10", b.ToConvertibleString());
            Assert.AreEqual("-1° 0' 0\" 10° 0' 0\"", b.ToDegreesMinutesSeconds());

            var c = new LatLongCoordinate(1, -10);
            Assert.AreEqual("N1 00.000 W10 00.000", c.ToString());
            Assert.AreEqual("N1 00.000 W10 00.000", c.ToString("g"));
            Assert.AreEqual("1, -10", c.ToConvertibleString());
            Assert.AreEqual("1° 0' 0\" -10° 0' 0\"", c.ToDegreesMinutesSeconds());

            var d = new LatLongCoordinate(-1, -10);
            Assert.AreEqual("S1 00.000 W10 00.000", d.ToString());
            Assert.AreEqual("S1 00.000 W10 00.000", d.ToString("g"));
            Assert.AreEqual("-1, -10", d.ToConvertibleString());
            Assert.AreEqual("-1° 0' 0\" -10° 0' 0\"", d.ToDegreesMinutesSeconds());
        }

        [TestMethod]
        public void TestFromConvertible()
        {
            var d = LatLongCoordinate.FromConvertibleString("-1, -10");
            Assert.AreEqual(-1, d.Latitude);
            Assert.AreEqual(-10, d.Longitude);
        }

        [TestMethod]
        public void TestParse()
        {
            var d = LatLongCoordinate.Parse("n 54 30 w 100 15");
            Assert.AreEqual(54.5, d.Latitude);
            Assert.AreEqual(-100.25, d.Longitude);
        }

        [TestMethod]
        public void TestFromDecimalDegrees()
        {
            var d = LatLongCoordinate.FromDecimalDegrees("N54.5 W100.25");
            Assert.AreEqual(54.5, d.Latitude);
            Assert.AreEqual(-100.25, d.Longitude);
        }

        [TestMethod]
        public void TestToBcGeographicSystem()
        {
            var a = new LatLongCoordinate(54, 112);
            var bc = a.ToBcNtsGridSystem();
            Assert.IsNotNull(bc);
            Assert.AreEqual(73, bc.Block);
            Assert.AreEqual(72, bc.MapArea);
            Assert.AreEqual(65, bc.QuarterUnit);
            Assert.AreEqual(83, bc.Series);
            Assert.AreEqual(16, bc.Sheet);
            Assert.AreEqual(101, bc.Unit);

            var bc2 = LatLongCoordinate.ToBcNtsGridSystem(a);
            Assert.AreEqual(bc, bc2);
        }

        [TestMethod]
        public void TestDirectionTo()
        {
            var a = new LatLongCoordinate(30, 30);
            var b = new LatLongCoordinate(30, 30);

            Assert.AreEqual(Angle.FromRadians(0), a.DirectionTo(b));
            Assert.AreEqual(Angle.FromRadians(0), b.DirectionTo(a));

            var c = new LatLongCoordinate(0, 0);

            Assert.AreEqual(Angle.FromRadians(3.903035f).Degrees, a.DirectionTo(c).Degrees, 0.0001);
            Assert.AreEqual(Angle.FromRadians(0.7614422f).Degrees, c.DirectionTo(a).Degrees, 0.0001);
        }

        [TestMethod]
        public void TestDistanceTo()
        {
            var a = new LatLongCoordinate(30, 30);
            var b = new LatLongCoordinate(40, 31);

            var d = a.DistanceTo(b, ReferenceEllipsoid.Wgs84);
            Assert.AreEqual(1113141.309, d, 0.0000001);

            d = a.SphereDistanceTo(b);
            Assert.AreEqual(1116899.78905404, d, 0.0000001);
        }

        [TestMethod]
        public void TestGreatCircleAngle()
        {
            var a = new LatLongCoordinate(30, 30);
            var b = new LatLongCoordinate(40, 31);

            var d = LatLongCoordinate.GreatCircleAngle(a, b);

            Assert.AreEqual(10.0332813262939, d.Degrees, 0.00000001);
        }

        [TestMethod]
        public void TestBcGeographicSystem()
        {
            var a = new LatLongCoordinate(51, -114);
            var nts = a.ToBcNtsGridSystem();

            Assert.AreEqual(76, nts.Block);
            Assert.AreEqual(73, nts.MapArea);
            Assert.AreEqual(65, nts.QuarterUnit);
            Assert.AreEqual(82, nts.Series);
            Assert.AreEqual(13, nts.Sheet);
            Assert.AreEqual(111, nts.Unit);
        }

        [TestMethod]
        public void TestDlsSystem()
        {
            var a = new LatLongCoordinate(54, -115);
            var dls = a.ToDlsSystem();

            Assert.AreEqual(87, dls.Direction);
            Assert.AreEqual(4, dls.LegalSubdivision);
            Assert.AreEqual(5, dls.Meridian);
            Assert.AreEqual("SW", dls.Quarter);
            Assert.AreEqual(26, dls.Range);
            Assert.AreEqual(6, dls.Section);
            Assert.AreEqual(75, dls.Township);

            var dls2 = LatLongCoordinate.ToDlsSystem(a);
            Assert.AreEqual(dls, dls2);
        }
    }
}
