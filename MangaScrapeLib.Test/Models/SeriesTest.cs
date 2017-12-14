using System;
using System.Collections.Generic;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class SeriesTest
    {
        public static IRepository TargetRepository => Repositories.EatManga;
        public const string ValidSeriesUri = "http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/";
        public const string ValidTitle = "SomeTitle";

        public static IEnumerable<object[]> CreateFromDataWorksData()
        {
            yield return new object[] { ValidSeriesUri, ValidTitle, true };
            yield return new object[] { null, ValidTitle, false };
            yield return new object[] { "http://omg.lol/", ValidTitle, false };
            yield return new object[] { ValidSeriesUri, null, false };
            yield return new object[] { ValidSeriesUri, string.Empty, false };
            yield return new object[] { ValidSeriesUri, " ", false };
        }

        [Theory]
        [MemberData(nameof(CreateFromDataWorksData))]
        public void CreateFromDataWorks(string uri, string title, bool shouldSucceed)
        {
            var output = Repositories.GetSeriesFromData(new Uri(uri), title);
            if(shouldSucceed)
            {
                Assert.NotNull(output);
                Assert.Same(TargetRepository, output.ParentRepository);
            }
            else
            {
                Assert.Null(output);
            }
        }
    }
}
