using System;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class SeriesTest
    {
        private static IRepository TargetRepository => Repositories.EatManga;
        public static readonly Uri ValidSeriesUri = new Uri("http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/");

        [Fact]
        public void CreateFromDataWorks()
        {
            var output = Repositories.GetSeriesFromData(ValidSeriesUri, "SomeTitle");
            Assert.NotNull(output);
            Assert.Same(TargetRepository, output.ParentRepository);
        }

        [Fact]
        public void CreateFromDataRejectsUnsupportedUri()
        {
            var invalidUris = new Uri[] { null, new Uri("http://omg.lol/") };

            foreach (var i in invalidUris)
            {
                Assert.Null(Repositories.GetSeriesFromData(i, "SomeTitle"));
            }
        }

        [Fact]
        public void CreateFromDataRejectsInvalidTitle()
        {
            var invalidTitles = new string[] { null, string.Empty, "  " };
            foreach (var i in invalidTitles)
            {
                Assert.Null(Repositories.GetSeriesFromData(ValidSeriesUri, i));
            }
        }
    }
}
