using MangaScrapeLib.Test.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MangaScrapeLib.Test.Repository
{
    /// <summary>
    ///This is a test class for MangaRepositoryTest and is intended
    ///to contain all MangaRepositoryTest Unit Tests
    ///</summary>
    public abstract class MangaRepositoryTestBase : IClassFixture<WebCache>, IDisposable
    {
        private const string RootDir = "C:\\";
        private readonly HashSet<string> UniqueParsedValues = new HashSet<string>();

        protected abstract IRepository GenerateRepository(WebCache webClient);

        private readonly Lazy<IRepository> repository;
        private IRepository Repository => repository.Value;

        private CancellationTokenSource CTS { get; }

        public MangaRepositoryTestBase(WebCache client)
        {
            repository = new Lazy<IRepository>(GenerateRepository(client));
            CTS = new CancellationTokenSource();
        }

        public void Dispose()
        {
            CTS.Dispose();
        }

        [Fact]
        public async Task GettingDefaultSeriesWorks()
        {
            var series = await Repository.GetSeriesAsync(CTS.Token);
            Assert.NotEmpty(series);
            foreach (var i in series)
            {
                CheckParsedStringValidity(i.Title, true);
                Assert.NotNull(i.SeriesPageUri);
                CheckParsedStringValidity(i.SeriesPageUri.ToString(), true);
                CheckParsedStringValidity(i.SuggestPath(RootDir), true);
            }
        }

        [Fact]
        public async Task LoadingSeriesFromDataWorks()
        {
            var series = await Repository.GetSeriesAsync(CTS.Token);
            var selectedSeries = series.First(d => d.SeriesPageUri.Host == Repository.RootUri.Host);

            selectedSeries = Repositories.GetSeriesFromData(selectedSeries.SeriesPageUri, selectedSeries.Title);
            Assert.Equal(Repository.RootUri, selectedSeries.ParentRepository.RootUri);

            var chapters = await selectedSeries.GetChaptersAsync(CTS.Token);
            Assert.NotEmpty(chapters);
        }

        [Fact]
        public async Task GettingChaptersWorks()
        {
            var series = await Repository.GetSeriesAsync(CTS.Token);
            var selectedSeries = series.First(d => d.SeriesPageUri.Host == Repository.RootUri.Host);

            var chapters = await selectedSeries.GetChaptersAsync(CTS.Token);

            if (Repository.SupportsCover) Assert.NotNull(selectedSeries.CoverImageUri);
            if (Repository.SupportsAuthor) Assert.True(!string.IsNullOrEmpty(selectedSeries.Author));
            if (Repository.SupportsDescription) Assert.True(!string.IsNullOrEmpty(selectedSeries.Description));
            if (Repository.SupportsLastUpdateTime) Assert.True(!string.IsNullOrEmpty(selectedSeries.Updated));
            if (Repository.SupportsTags) Assert.True(!string.IsNullOrEmpty(selectedSeries.Tags));

            Assert.NotEmpty(chapters);
            var readingOrder = 0;
            foreach (var i in chapters)
            {
                CheckParsedStringValidity(i.Title, false);
                Assert.Equal(readingOrder, i.ReadingOrder);
                readingOrder++;

                Assert.NotEmpty(i.Updated);
                Assert.Same(selectedSeries, i.ParentSeries);
                Assert.NotNull(i.FirstPageUri);
                CheckParsedStringValidity(i.FirstPageUri.ToString(), true);

                CheckParsedStringValidity(i.SuggestPath(RootDir), false);
            }
        }

        [Fact]
        public async Task GettingPagesWorks()
        {
            var series = await Repository.GetSeriesAsync(CTS.Token);
            var selectedSeries = series.First(d => d.SeriesPageUri.Host == Repository.RootUri.Host);
            var chapters = await selectedSeries.GetChaptersAsync(CTS.Token);
            var selectedChapter = chapters.First();

            var pages = await selectedChapter.GetPagesAsync(CTS.Token);
            Assert.NotEmpty(pages);
            var ctr = 1;
            foreach (var i in pages)
            {
                Assert.NotNull(i.PageUri);
                CheckParsedStringValidity(i.PageUri.ToString(), false);

                Assert.Same(selectedChapter, i.ParentChapter);
                Assert.Equal(ctr, i.PageNumber);
                ctr++;
            }
        }

        [Fact]
        public async Task GettingPageImageWorks()
        {
            var series = await Repository.GetSeriesAsync(CTS.Token);
            var selectedSeries = series.First(d => d.SeriesPageUri.Host == Repository.RootUri.Host);
            var chapters = await selectedSeries.GetChaptersAsync(CTS.Token);
            var selectedChapter = chapters.First();
            var pages = await selectedChapter.GetPagesAsync(CTS.Token);
            var selectedPage = pages.First();

            var imageBytes = await selectedPage.GetImageAsync(CTS.Token);
            Assert.NotNull(selectedPage.ImageUri);
            Assert.NotEmpty(imageBytes);

            CheckParsedStringValidity(selectedPage.SuggestPath(RootDir), true);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("a", false)]
        [InlineData("one", true)]
        public async Task SearchWorks(string searchQuery, bool requiresResults)
        {
            var searchResult = await Repository.SearchSeriesAsync(searchQuery, CTS.Token);
            if (requiresResults)
            {
                Assert.NotEmpty(searchResult);
            }

            foreach (var i in searchResult)
            {
                Assert.Same(Repository, i.ParentRepository);
                CheckParsedStringValidity(i.Title, true);
                Assert.NotNull(i.SeriesPageUri);
                CheckParsedStringValidity(i.SeriesPageUri.ToString(), true);
                CheckParsedStringValidity(i.SuggestPath(RootDir), true);
            }
        }

        [Fact]
        public async Task NoResultsSearchWorks()
        {
            var searchQuery = "fbywguewvugewf";
            var searchResult = await Repository.SearchSeriesAsync(searchQuery, CTS.Token);
            Assert.Empty(searchResult);
        }

        [Fact]
        public async Task GetSeriesCoverWorks()
        {
            if (!Repository.SupportsCover)
            {
                return;
            }

            var series = await Repository.GetSeriesAsync(CTS.Token);
            var selectedSeries = series.First(d => d.SeriesPageUri.Host == Repository.RootUri.Host);

            var coverImage = selectedSeries.GetCoverAsync();
            Assert.NotNull(coverImage);
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
            if (shouldBeUnique)
            {
                Assert.DoesNotContain(input, UniqueParsedValues);
                UniqueParsedValues.Add(input);
            }
        }
    }
}
