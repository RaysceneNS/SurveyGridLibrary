using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
{
    [TestClass]
    public class LatLongCoordinateTests
    {
        [TestMethod]
        public void TestConstructDms()
        {
            //Degrees/Minute/Seconds
            var latLongCoordinate = new LatLongCoordinate(50, 3, 4, 100, 1, 2);
            Assert.AreEqual(50.0511093139648, latLongCoordinate.Latitude, 0.00000001);
            Assert.AreEqual(100.017219543457, latLongCoordinate.Longitude, 0.00000001);
        }

        [TestMethod]
        public void TestConstructDm()
        {
            //Degrees/Minutes
            var latLongCoordinate = new LatLongCoordinate(50, 3, 100, 1);
            Assert.AreEqual(50.0499992370605, latLongCoordinate.Latitude, 0.00000001);
            Assert.AreEqual(100.016670227051, latLongCoordinate.Longitude, 0.00000001);
        }

        [TestMethod]
        public void TestFromRadians()
        {
            var latLongCoordinate = LatLongCoordinate.FromRadians((float) Math.PI, (float) Math.PI/2);

            Assert.AreEqual(90, latLongCoordinate.Latitude);
            Assert.AreEqual(90, latLongCoordinate.Longitude);

            Assert.AreEqual(Math.PI / 2, latLongCoordinate.RadiansLat, 0.000001);
            Assert.AreEqual(Math.PI / 2, latLongCoordinate.RadiansLon, 0.000001);
        }

        [TestMethod]
        public void TestClone()
        {
            var latLongCoordinate = LatLongCoordinate.FromRadians((float)Math.PI, (float)Math.PI / 2);
            var cloned = latLongCoordinate.Clone();
            Assert.AreNotSame(latLongCoordinate, cloned);
            Assert.AreEqual(latLongCoordinate, cloned);
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
        }

        [TestMethod]
        public void TestInequality()
        {
            var a = new LatLongCoordinate(1, 10);
            var b = new LatLongCoordinate(2, 10);

            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
        }

        [TestMethod]
        public void TestHashCode()
        {
            Assert.IsTrue(new LatLongCoordinate(1, 10).GetHashCode() == new LatLongCoordinate(1, 10).GetHashCode());
        }

        [TestMethod]
        public void TestToDmsString()
        {
            var a = new LatLongCoordinate(1, 10);
            Assert.AreEqual("1, 10", a.ToString());
            Assert.AreEqual("1, 10", a.ToString("g"));
            Assert.AreEqual("N1 00.000 E10 00.000", a.ToString("dm"));
            Assert.AreEqual("N1° 0' 0.0\" E10° 0' 0.0\"", a.ToString("dms"));

            var b = new LatLongCoordinate(-1, 10);
            Assert.AreEqual("-1, 10", b.ToString());
            Assert.AreEqual("-1, 10", b.ToString("g"));
            Assert.AreEqual("S1 00.000 E10 00.000", b.ToString("dm"));
            Assert.AreEqual("S1° 0' 0.0\" E10° 0' 0.0\"", b.ToString("dms"));

            var c = new LatLongCoordinate(1, -10);
            Assert.AreEqual("1, -10", c.ToString());
            Assert.AreEqual("1, -10", c.ToString("g"));
            Assert.AreEqual("N1 00.000 W10 00.000", c.ToString("dm"));
            Assert.AreEqual("N1° 0' 0.0\" W10° 0' 0.0\"", c.ToString("dms"));

            var d = new LatLongCoordinate(-1, -10);
            Assert.AreEqual("-1, -10", d.ToString());
            Assert.AreEqual("-1, -10", d.ToString("g"));
            Assert.AreEqual("S1 00.000 W10 00.000", d.ToString("dm"));
            Assert.AreEqual("S1° 0' 0.0\" W10° 0' 0.0\"", d.ToString("dms"));
        }

        [TestMethod]
        public void TestFromConvertible()
        {
            var latLongCoordinate = LatLongCoordinate.FromConvertibleString("-1, -10");
            Assert.AreEqual(-1, latLongCoordinate.Latitude);
            Assert.AreEqual(-10, latLongCoordinate.Longitude);
        }

        [TestMethod]
        public void TestParse()
        {
            var latLongCoordinate = LatLongCoordinate.Parse("n 54 30 w 100 15");
            Assert.AreEqual(54.5, latLongCoordinate.Latitude);
            Assert.AreEqual(-100.25, latLongCoordinate.Longitude);
        }

        [TestMethod]
        public void TestFromDecimalDegrees()
        {
            var latLongCoordinate = LatLongCoordinate.FromDecimalDegrees("N54.5 W100.25");
            Assert.AreEqual(54.5, latLongCoordinate.Latitude);
            Assert.AreEqual(-100.25, latLongCoordinate.Longitude);
        }

        [TestMethod]
        public void TestToBcGeographicSystem()
        {
            var bc = new LatLongCoordinate(54, -112).ToBcNtsGridSystem();
            Assert.IsNotNull(bc);

            Assert.AreEqual('A', bc.QuarterUnit);
            Assert.AreEqual(1, bc.Unit);
            Assert.AreEqual('A', bc.Block);

            Assert.AreEqual(83, bc.Series);
            Assert.AreEqual('I', bc.MapArea);
            Assert.AreEqual(1, bc.Sheet);
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
            
            Assert.AreEqual(1113141.546, a.DistanceTo(b, ReferenceEllipsoid.Wgs84), 0.0000001);

        }
        [TestMethod]
        public void TestSphereDistanceTo()
        {
            var a = new LatLongCoordinate(30, 30);
            var b = new LatLongCoordinate(40, 31);
            
            Assert.AreEqual(1116900.07417898, a.SphereDistanceTo(b), 0.0000001);
        }

        [TestMethod]
        public void TestGreatCircleAngle()
        {
            var a = new LatLongCoordinate(30, 30);
            var b = new LatLongCoordinate(40, 31);

            var d = LatLongCoordinate.GreatCircleAngle(a, b);

            Assert.AreEqual(10.033284074692, d.Degrees, 0.00000001);
        }


        [TestMethod]
        public void TestBcGeographicSystem1()
        {
            // A-1-A/82-O-1
            var a = new LatLongCoordinate(51, -114);
            var nts = a.ToBcNtsGridSystem();

            Assert.AreEqual('A', nts.QuarterUnit);
            Assert.AreEqual(1, nts.Unit);
            Assert.AreEqual('A', nts.Block);

            Assert.AreEqual(82, nts.Series);
            Assert.AreEqual('O', nts.MapArea);
            Assert.AreEqual(1, nts.Sheet);
        }

        [TestMethod]
        public void TestBcGeographicSystem2()
        {
            // A-1-A/83-J-3
            var nts = new LatLongCoordinate(54, -115).ToBcNtsGridSystem();

            Assert.AreEqual('A', nts.QuarterUnit);
            Assert.AreEqual(1, nts.Unit);
            Assert.AreEqual('A', nts.Block);

            Assert.AreEqual(83, nts.Series);
            Assert.AreEqual('J', nts.MapArea);
            Assert.AreEqual(3, nts.Sheet);
        }


        [TestMethod]
        public void TestDlsSystemNearFifth()
        {
            var coordinate = new LatLongCoordinate(51, -114);

            var dls = LatLongCoordinate.ToDlsSystem(coordinate);
            //5-33-23-29-W4
            Assert.AreEqual(23, dls.Township);
            Assert.AreEqual(29, dls.Range);
            Assert.AreEqual('W', dls.Direction);
            Assert.AreEqual(4, dls.Meridian);

            Assert.AreEqual(33, dls.Section);
            Assert.AreEqual("SW", dls.Quarter);
            Assert.AreEqual(5, dls.LegalSubdivision);
        }

        [TestMethod]
        public void TestDlsSystem2()
        {
            var dls = new LatLongCoordinate(54, -115).ToDlsSystem();
            //9-8-58-7-W5
            Assert.AreEqual('W', dls.Direction);
            Assert.AreEqual(5, dls.Meridian);
            Assert.AreEqual(58, dls.Township);
            Assert.AreEqual(7, dls.Range);

            Assert.AreEqual(8, dls.Section);
            Assert.AreEqual("NE", dls.Quarter);
            Assert.AreEqual(9, dls.LegalSubdivision);
        }
    }
}
