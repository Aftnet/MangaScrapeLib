using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MangaScrapeLib.Test.Repository
{
    /// <summary>
    ///This is a test class for MangaRepositoryTest and is intended
    ///to contain all MangaRepositoryTest Unit Tests
    ///</summary>
    public abstract class MangaRepositoryTestBase
    {
        private const string RootDir = "C:\\";
        private readonly HashSet<string> UniqueParsedValues = new HashSet<string>();

        protected abstract IRepository Repository { get; }

        [Fact]
        public async Task ParsingWorks()
        {
            var series = await Repository.GetSeriesAsync();
            Assert.True(series.Any());
            foreach (var i in series)
            {
                Assert.Same(Repository, i.ParentRepository);
                CheckParsedStringValidity(i.Title, true);
                Assert.NotNull(i.SeriesPageUri);
                CheckParsedStringValidity(i.SeriesPageUri.ToString(), true);
                CheckParsedStringValidity(i.SuggestPath(RootDir), true);
            }

            UniqueParsedValues.Clear();

            var selectedSeries = series.First();
            selectedSeries = Repository.GetSeriesFromData(selectedSeries.SeriesPageUri, selectedSeries.Title);
            var chapters = await selectedSeries.GetChaptersAsync();

            if (Repository.SupportsCover) Assert.NotNull(selectedSeries.CoverImageUri);
            if (Repository.SupportsAuthor) Assert.True(!string.IsNullOrEmpty(selectedSeries.Author));
            if (Repository.SupportsDescription) Assert.True(!string.IsNullOrEmpty(selectedSeries.Description));
            if (Repository.SupportsLastUpdateTime) Assert.True(!string.IsNullOrEmpty(selectedSeries.Updated));
            if (Repository.SupportsTags) Assert.True(!string.IsNullOrEmpty(selectedSeries.Tags));

            Assert.True(chapters.Any());
            foreach (var i in chapters)
            {
                CheckParsedStringValidity(i.Title, true);
                Assert.False(string.IsNullOrEmpty(i.Updated));
                Assert.Same(selectedSeries, i.ParentSeries);
                Assert.NotNull(i.FirstPageUri);
                CheckParsedStringValidity(i.FirstPageUri.ToString(), true);

                CheckParsedStringValidity(i.SuggestPath(RootDir), true);
            }

            //Some repositories have a chapter's first page placed at chapter page. Clear uniqies values before testing.
            UniqueParsedValues.Clear();

            var selectedChapter = chapters[0];
            var pages = await selectedChapter.GetPagesAsync();
            Assert.True(pages.Any());
            var ctr = 1;
            foreach (var i in pages)
            {
                Assert.NotNull(i.PageUri);
                CheckParsedStringValidity(i.PageUri.ToString(), false);

                Assert.Same(selectedChapter, i.ParentChapter);
                Assert.Equal(ctr, i.PageNo);
                ctr++;
            }

            var selectedPage = pages[0];
            var imageBytes = await selectedPage.GetImageAsync();
            Assert.NotNull(imageBytes);
            Assert.True(imageBytes.Any());
            Assert.NotNull(selectedPage.ImageUri);

            CheckParsedStringValidity(selectedPage.SuggestPath(RootDir), true);
        }

        [Fact]
        public async Task SearchWorks()
        {
            var searchQuery = "one";
            var searchResult = await Repository.SearchSeriesAsync(searchQuery);
            Assert.True(searchResult.Any());
            foreach (var i in searchResult)
            {
                Assert.Same(Repository, i.ParentRepository);
                CheckParsedStringValidity(i.Title, true);
                Assert.True(i.Title.ToLowerInvariant().Contains(searchQuery));
                Assert.NotNull(i.SeriesPageUri);
                CheckParsedStringValidity(i.SeriesPageUri.ToString(), true);

                CheckParsedStringValidity(i.SuggestPath(RootDir), true);
            }
        }

        [Fact]
        public async Task NoResultsSearchWorks()
        {
            var searchQuery = "fbywguewvugewf";
            var searchResult = await Repository.SearchSeriesAsync(searchQuery);
            Assert.False(searchResult.Any());
        }

        [Fact]
        public void NameIsSet()
        {
            Assert.NotNull(Repository.Name);
        }

        [Fact]
        public void RootUriIsSet()
        {
            Assert.NotNull(Repository.RootUri);
        }

        [Fact]
        public void IconLoadingWorks()
        {
            Assert.NotNull(Repository.Icon);
        }

        private void CheckParsedStringValidity(string input, bool shouldBeUnique)
        {
            Assert.False(string.IsNullOrEmpty(input));
            Assert.False(string.IsNullOrWhiteSpace(input));
            if(shouldBeUnique)
            {
                Assert.False(UniqueParsedValues.Contains(input), "Duplicate value detected when parsing");
                UniqueParsedValues.Add(input);
            }
        }
    }
}
