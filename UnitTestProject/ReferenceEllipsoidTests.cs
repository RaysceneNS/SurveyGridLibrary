using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurveyGridLibrary.Test
{
    [TestClass]
    public class ReferenceEllipsoidTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(ReferenceEllipsoid.Wgs84);
            Assert.IsNotNull(ReferenceEllipsoid.Clarke1866);
            Assert.IsNotNull(ReferenceEllipsoid.Grs80);
        }

        [TestMethod]
        public void TestInverseFlattening()
        {
            Assert.AreEqual(0.00335281066474748, ReferenceEllipsoid.Wgs84.InverseFlattening, 0.00000000000000001);
            Assert.AreEqual(0.00339007530392762, ReferenceEllipsoid.Clarke1866.InverseFlattening, 0.00000000000000001);
            Assert.AreEqual(0.00335281068118232, ReferenceEllipsoid.Grs80.InverseFlattening, 0.00000000000000001);
        }
    }
}
