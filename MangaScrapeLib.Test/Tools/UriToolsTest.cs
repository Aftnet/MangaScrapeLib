using Xunit;
using MangaScrapeLib.Tools;

namespace MangaScrapeLib.Test.Tools
{
    public class UriToolsTest
    {
        [Fact]
        public void TruncateLastUriSegmentRemovesLastSegment()
        {
            var testUri = "http://first.com/second/third";
            Assert.Equal(ExtensionMethods.TruncateLastUriSegment(testUri), "http://first.com/second/");
        }

        [Fact]
        public void GetLastUriSegmentGetsLastSegment()
        {
            var testUri = "http://first.com/second/third";
            Assert.Equal(ExtensionMethods.GetLastUriSegment(testUri), "third");
        }
    }
}
