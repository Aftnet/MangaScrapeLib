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

        protected Repository Repository { get; set; }

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
                CheckParsedStringValidity(i.Title);
                Assert.IsNotNull(i.SeriesPageUri);
                CheckParsedStringValidity(i.SeriesPageUri.ToString());

                Assert.IsFalse(string.IsNullOrEmpty(i.Updated));
                Assert.IsNull(i.CoverImageUri);
                Assert.IsNull(i.Description);

                CheckParsedStringValidity(i.SuggestPath(RootDir));
            }

            var selectedSeries = series[0];
            var chapters = await selectedSeries.GetChaptersAsync();

            var metadataSupport = Repository.SeriesMetadata;
            if (metadataSupport.Cover) Assert.IsNotNull(selectedSeries.CoverImageUri);
            if (metadataSupport.Author) Assert.IsTrue(!string.IsNullOrEmpty(selectedSeries.Author));
            if (metadataSupport.Description) Assert.IsTrue(!string.IsNullOrEmpty(selectedSeries.Description));
            if (metadataSupport.Release) Assert.IsTrue(!string.IsNullOrEmpty(selectedSeries.Release));
            if (metadataSupport.Tags) Assert.IsTrue(!string.IsNullOrEmpty(selectedSeries.Tags));

            Assert.IsTrue(chapters.Any());
            foreach (var i in chapters)
            {
                CheckParsedStringValidity(i.Title);
                Assert.IsFalse(string.IsNullOrEmpty(i.Updated));
                Assert.AreSame(selectedSeries, i.ParentSeries);
                Assert.IsNotNull(i.FirstPageUri);
                CheckParsedStringValidity(i.FirstPageUri.ToString());

                CheckParsedStringValidity(i.SuggestPath(RootDir));
            }

            //Some repositories have a chapter's first page placed at chapter page. Clear uniqies values before testing.
            UniqueParsedValues.Clear();

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
        public async Task SearchWorks()
        {
            var searchQuery = "naruto";
            var searchResult = await Repository.SearchSeriesAsync(searchQuery);
            Assert.IsTrue(searchResult.Any());
            foreach (var i in searchResult)
            {
                Assert.AreSame(Repository, i.ParentRepository);
                CheckParsedStringValidity(i.Title);
                Assert.IsTrue(i.Title.ToLower().Contains(searchQuery));
                Assert.IsNotNull(i.SeriesPageUri);
                CheckParsedStringValidity(i.SeriesPageUri.ToString());

                Assert.IsFalse(string.IsNullOrEmpty(i.Updated));
                Assert.IsNull(i.CoverImageUri);
                Assert.IsNull(i.Description);

                CheckParsedStringValidity(i.SuggestPath(RootDir));
            }
        }

        [TestMethod]
        public async Task NoResultsSearchWorks()
        {
            var searchQuery = "fbywguewvugewf";
            var searchResult = await Repository.SearchSeriesAsync(searchQuery);
            Assert.IsFalse(searchResult.Any());
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
