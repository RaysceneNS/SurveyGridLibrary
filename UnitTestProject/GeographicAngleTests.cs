using GisLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class GeographicAngleTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dms = new GeographicAngle(40.714);

            Assert.AreEqual(40, dms.Degrees());
        }
    }
}
