using System;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class ChapterTest
    {
        private static IRepository TargetRepository => Repositories.EatManga;
        private static readonly Uri ValidChapterUri = new Uri("http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/testch/");

        private ISeries TestSeries = TargetRepository.GetSeriesFromData(SeriesTest.ValidSeriesUri, "SomeTitle");

        [Fact]
        public void CreateFromDataWorks()
        {
            Assert.NotNull(TestSeries.GetSingleChapterFromData(ValidChapterUri, "Test chapter"));
        }

        [Fact]
        public void CreateFromDataRejectsUnsupportedUri()
        {
            var invalidUris = new Uri[] { null, new Uri("http://omg.lol/") };
            foreach (var i in invalidUris)
            {
                Assert.Null(TestSeries.GetSingleChapterFromData(i, "Test chapter"));
            }
        }

        [Fact]
        public void CreateFromDataRejectsInvalidTitle()
        {
            var invalidTitles = new string[] { null, string.Empty, "  " };
            foreach (var i in invalidTitles)
            {
                Assert.Null(TestSeries.GetSingleChapterFromData(ValidChapterUri, i));
            }
        }
    }
}
