using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class ChapterTest
    {
        private static IRepository TargetRepository => Repositories.AllRepositories.First(d => d.RootUri.Host.Contains("eatmanga"));
        private const string ValidChapterUri = "http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/testch/";
        private const string ValidChapterTitle = "Test chapter";

        private ISeries TestSeries = Repositories.GetSeriesFromData(new Uri(SeriesTest.ValidSeriesUri), "SomeTitle");

        public static IEnumerable<object[]> CreateFromDataWorksData()
        {
            yield return new object[] { ValidChapterUri, ValidChapterTitle, true };
            yield return new object[] { null, ValidChapterTitle, false };
            yield return new object[] { "http://omg.lol/", ValidChapterTitle, false };
            yield return new object[] { ValidChapterUri, null, false };
            yield return new object[] { ValidChapterUri, string.Empty, false };
            yield return new object[] { ValidChapterUri, " ", false };
        }

        [Theory]
        [MemberData(nameof(CreateFromDataWorksData))]
        public void CreateFromDataWorks(string uri, string title, bool shouldSucceed)
        {
            var output = TestSeries.GetSingleChapterFromData(uri == null ? null : new Uri(uri), title);
            if (shouldSucceed)
            {
                Assert.NotNull(output);
                Assert.Same(TestSeries, output.ParentSeries);
            }
            else
            {
                Assert.Null(output);
            }
        }
    }
}
