using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
{
    [TestClass]
    public class UwiTests
    {
        [TestMethod]
        public void TestEquality()
        {
            var a = new UniqueWellIdentifier('0', 'd', 96, 'h', 94, 'a', 15, '0');
            var b = new UniqueWellIdentifier('0', 'd', 96, 'h', 94, 'a', 15, '0');

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));

            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
        }

        [TestMethod]
        public void TestNtsConstructor()
        {
            var a = new UniqueWellIdentifier('0', 'd', 96, 'h', 94, 'a', 15, '0');
            Assert.IsNotNull(a);
        }

        [TestMethod]
        public void TestDlsConstructor()
        {

            var b = new UniqueWellIdentifier("00", 0, 0, 0, 0, 'A', 0, 'A');
            Assert.IsNotNull(b);
        }

        [TestMethod]
        public void TestNtsToString()
        {
            var a = new UniqueWellIdentifier('0', 'd', 96, 'h', 94, 'a', 15, '0');
            Assert.AreEqual("200d096h094a1500", a.ToString());
        }

        [TestMethod]
        public void TestDlsToString()
        {
            var a = new UniqueWellIdentifier("00", 0, 0, 0, 0, 'A', 0, 'A');
            Assert.AreEqual("100000000000A00A", a.ToString());
        }

        [TestMethod]
        public void TestParseDls()
        {
            var a = UniqueWellIdentifier.Parse("100143608517W600");
            Assert.AreEqual(UniqueWellIdentifier.SurveySystemCode.DominionLandSurvey, a.SurveySystem);
            Assert.AreEqual("00", a.LocationExceptionCode);
            Assert.AreEqual("143608517W60", a.LegalSurveySystem);
            Assert.AreEqual('0', a.EventSequenceCode);

            var b = UniqueWellIdentifier.Parse("100162512315W602");
            Assert.AreEqual(UniqueWellIdentifier.SurveySystemCode.DominionLandSurvey, b.SurveySystem);
        }

        [TestMethod]
        public void TestParseNts()
        {
            var wellIdentifier = UniqueWellIdentifier.Parse("200D096H094A1500");
            Assert.AreEqual(UniqueWellIdentifier.SurveySystemCode.NationalTopographicSeries, wellIdentifier.SurveySystem);
            Assert.AreEqual("00", wellIdentifier.LocationExceptionCode);
            Assert.AreEqual("D096H094A150", wellIdentifier.LegalSurveySystem);
            Assert.AreEqual('0', wellIdentifier.EventSequenceCode);
        }

        [TestMethod]
        public void TestParseFederal()
        {
            var wellIdentifier = UniqueWellIdentifier.Parse("300F556220121450");
            Assert.AreEqual(UniqueWellIdentifier.SurveySystemCode.FederalPermitSystem, wellIdentifier.SurveySystem);
            Assert.AreEqual("00", wellIdentifier.LocationExceptionCode);
            Assert.AreEqual("F55622012145", wellIdentifier.LegalSurveySystem);
            Assert.AreEqual('0', wellIdentifier.EventSequenceCode);
        }

        [TestMethod]
        public void TestParseGeodetic()
        {
            //400/ 79.999 / 104.999 / 00
            var wellIdentifier = UniqueWellIdentifier.Parse("4007999910499900");
            Assert.AreEqual(UniqueWellIdentifier.SurveySystemCode.GeodeticCoordinates, wellIdentifier.SurveySystem);
            Assert.AreEqual("00", wellIdentifier.LocationExceptionCode);
            Assert.AreEqual("799991049990", wellIdentifier.LegalSurveySystem);
            Assert.AreEqual('0', wellIdentifier.EventSequenceCode);
        }

        [TestMethod]
        public void TestGeodeticToLatLong()
        {
            //400/ 79.999 / 104.999 / 00
            var a = UniqueWellIdentifier.Parse("4007999910499900");
            var ll = UniqueWellIdentifier.ToLatLongCoordinate(a);
            Assert.AreEqual(79.999, ll.Latitude, 0.001);
            Assert.AreEqual(104.999, ll.Longitude, 0.001);
        }

        [TestMethod]
        public void TestNtsToLatLong()
        {
            var a = UniqueWellIdentifier.Parse("200D096H094A1500");
            var latLongCoordinate = UniqueWellIdentifier.ToLatLongCoordinate(a);
            Assert.AreEqual(-120.5656280f, latLongCoordinate.Longitude, 0.0000001);
            Assert.AreEqual(56.914585f, latLongCoordinate.Latitude, 0.000001);
        }

        [TestMethod, ExpectedException(typeof(CoordinateConversionException))]
        public void TestDlsToLatLongWithException()
        {
            var a = UniqueWellIdentifier.Parse("100143608517W600");
            var latLongCoordinate = UniqueWellIdentifier.ToLatLongCoordinate(a);
            Assert.AreEqual(56.418849, latLongCoordinate.Latitude, 0.000001);
            Assert.AreEqual(-120.512955, latLongCoordinate.Longitude, 0.000001);
        }

        [TestMethod]
        public void TestDlsToLatLong()
        {
            var a = UniqueWellIdentifier.Parse("100143608517W500");
            var latLongCoordinate = UniqueWellIdentifier.ToLatLongCoordinate(a);
            Assert.AreEqual(56.4191398, latLongCoordinate.Latitude, 0.000001);
            Assert.AreEqual(-116.5479126, latLongCoordinate.Longitude, 0.000001);
        }

        [TestMethod]
        public void TestFpsToLatLong()
        {
            var a = UniqueWellIdentifier.Parse("300F556220121450");
            var latLongCoordinate = UniqueWellIdentifier.ToLatLongCoordinate(a);
            Assert.AreEqual(62.404167, latLongCoordinate.Latitude, 0.000001);
            Assert.AreEqual(-121.921875, latLongCoordinate.Longitude, 0.000001);
        }
    }
}
