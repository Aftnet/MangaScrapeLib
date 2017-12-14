using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class SeriesTest
    {
        public static IRepository TargetRepository => Repositories.EatManga;
        public const string ValidSeriesUri = "http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/";
        public const string ValidSeriesTitle = "SomeTitle";

        public static IEnumerable<object[]> CreateFromDataWorksData()
        {
            yield return new object[] { ValidSeriesUri, ValidSeriesTitle, true };
            yield return new object[] { null, ValidSeriesTitle, false };
            yield return new object[] { "http://omg.lol/", ValidSeriesTitle, false };
            yield return new object[] { ValidSeriesUri, null, false };
            yield return new object[] { ValidSeriesUri, string.Empty, false };
            yield return new object[] { ValidSeriesUri, " ", false };
        }

        [Theory]
        [MemberData(nameof(CreateFromDataWorksData))]
        public void CreateFromDataWorks(string uri, string title, bool shouldSucceed)
        {
            var output = Repositories.GetSeriesFromData(uri == null ? null : new Uri(uri), title);
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

        [Fact]
        public void RepositoriesAreSet()
        {
            var repositories = Repositories.AllRepositories;
            Assert.NotEmpty(repositories);
            Assert.DoesNotContain(repositories, d => d == null);
        }
    }
}
