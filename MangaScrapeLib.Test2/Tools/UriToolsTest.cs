using MangaScrapeLib.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.Test.Tools
{
    [TestClass]
    public class UriToolsTest
    {
        [TestMethod]
        public void TruncateLastUriSegmentRemovesLastSegment()
        {
            var testUri = "http://first.com/second/third";
            Assert.AreEqual(UriTools.TruncateLastUriSegment(testUri), "http://first.com/second/");
        }

        [TestMethod]
        public void GetLastUriSegmentGetsLastSegment()
        {
            var testUri = "http://first.com/second/third";
            Assert.AreEqual(UriTools.GetLastUriSegment(testUri), "third");
        }
    }
}
