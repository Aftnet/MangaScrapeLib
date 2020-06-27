using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class ChapterTest
    {
        public static Uri ValidChapterUri { get; } = new Uri(SeriesTest.ValidSeriesUri, "/chapter/path");
        public static string ValidChapterTitle { get; } = "Test chapter";

        private static ISeries TestSeries { get; } = Repositories.GetSeriesFromData(SeriesTest.ValidSeriesUri, SeriesTest.ValidSeriesTitle);

        public static IEnumerable<object[]> CreateFromDataWorksData()
        {
            yield return new object[] { ValidChapterUri, ValidChapterTitle, true };
            yield return new object[] { default(Uri), ValidChapterTitle, false };
            yield return new object[] { new Uri("http://omg.lol/"), ValidChapterTitle, false };
            yield return new object[] { ValidChapterUri, default(string), false };
            yield return new object[] { ValidChapterUri, string.Empty, false };
            yield return new object[] { ValidChapterUri, " ", false };
        }

        [Theory]
        [MemberData(nameof(CreateFromDataWorksData))]
        public void CreateFromDataWorks(Uri uri, string title, bool shouldSucceed)
        {
            var output = TestSeries.GetSingleChapterFromData(uri, title, -1);
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
