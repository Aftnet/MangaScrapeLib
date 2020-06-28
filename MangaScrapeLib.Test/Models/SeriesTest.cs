using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class SeriesTest
    {
        public static IRepository TargetRepository { get; } = Repositories.AllRepositories.First();
        public static Uri ValidSeriesUri { get; } = new Uri(TargetRepository.RootUri, "some/path");
        public static string ValidSeriesTitle { get; } = "SomeTitle";

        public static IEnumerable<object[]> CreateFromDataWorksData()
        {
            yield return new object[] { ValidSeriesUri, ValidSeriesTitle, true };
            yield return new object[] { default(Uri), ValidSeriesTitle, false };
            yield return new object[] { new Uri("http://omg.lol/"), ValidSeriesTitle, false };
            yield return new object[] { ValidSeriesUri, default(string), false };
            yield return new object[] { ValidSeriesUri, string.Empty, false };
            yield return new object[] { ValidSeriesUri, " ", false };
        }

        [Theory]
        [MemberData(nameof(CreateFromDataWorksData))]
        public void CreateFromDataWorks(Uri uri, string title, bool shouldSucceed)
        {
            var output = Repositories.GetSeriesFromData(uri, title);
            if (shouldSucceed)
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
