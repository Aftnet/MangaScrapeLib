using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Test.Repositories
{
    /// <summary>
    ///This is a test class for MangaRepositoryTest and is intended
    ///to contain all MangaRepositoryTest Unit Tests
    ///</summary>
    [TestClass]
    public abstract class MangaRepositoryTestBase
    {
        protected abstract Repository GetRepository();

        private const string RootDir = "C:\\";
        private readonly HashSet<string> UniqueParsedValues = new HashSet<string>();

        private Repository Repository { get; set; }

        [TestInitialize]
        public void TestInitializer()
        {
            Repository = GetRepository();
        }

        [TestMethod]
        public async Task ParsingWorks()
        {
            var series = await Repository.GetSeriesAsync();
            Assert.IsTrue(series.Any());
            foreach (var i in series)
            {
                Assert.AreSame(Repository, i.ParentRepository);
                CheckParsedStringValidity(i.Name);
                Assert.IsNotNull(i.SeriesPageUri);
                CheckParsedStringValidity(i.SeriesPageUri.ToString());

                Assert.IsNull(i.CoverImageUri);
                Assert.IsNull(i.Description);
                Assert.IsNull(i.Tags);

                CheckParsedStringValidity(i.SuggestPath(RootDir));
            }

            var selectedSeries = series[0];
            var chapters = await selectedSeries.GetChaptersAsync();

            Assert.IsTrue(chapters.Any());
            foreach (var i in chapters)
            {
                CheckParsedStringValidity(i.Title);
                Assert.AreSame(selectedSeries, i.ParentSeries);
                Assert.IsNotNull(i.FirstPageUri);
                CheckParsedStringValidity(i.FirstPageUri.ToString());

                CheckParsedStringValidity(i.SuggestPath(RootDir));
            }

            var selectedChapter = chapters[0];
            var pages = await selectedChapter.GetPagesAsync();
            Assert.IsTrue(pages.Any());
            var ctr = 1;
            foreach (var i in pages)
            {
                Assert.IsNotNull(i.PageUri);
                CheckParsedStringValidity(i.PageUri.ToString());

                Assert.AreSame(selectedChapter, i.ParentChapter);
                Assert.AreEqual(ctr, i.PageNo);
                ctr++;
            }

            var selectedPage = pages[0];
            var imageBytes = await selectedPage.GetImageAsync();
            Assert.IsNotNull(imageBytes);
            Assert.IsTrue(imageBytes.Any());
            Assert.IsNotNull(selectedPage.ImageUri);

            CheckParsedStringValidity(selectedPage.SuggestPath(RootDir));
        }

        [TestMethod]
        public void MangaIndexPageIsSet()
        {
            Uri actual;
            actual = Repository.MangaIndexPage;
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void NameIsSet()
        {
            Assert.IsNotNull(Repository.Name);
        }

        [TestMethod]
        public void RootUriIsSet()
        {
            Assert.IsNotNull(Repository.RootUri);
        }

        [TestMethod]
        public void IconLoadingWorks()
        {
            Assert.IsNotNull(Repository.Icon);
        }

        private void CheckParsedStringValidity(string input)
        {
            Assert.IsFalse(string.IsNullOrEmpty(input));
            Assert.IsFalse(string.IsNullOrWhiteSpace(input));
            Assert.IsFalse(UniqueParsedValues.Contains(input), "Duplicate value detected when parsing");
            UniqueParsedValues.Add(input);
        }
    }
}
