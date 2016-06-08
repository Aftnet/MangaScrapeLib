using MangaScrapeLib.Models;
using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaScrapeLib.TestServices
{
    /// <summary>
    ///This is a test class for MangaRepositoryTest and is intended
    ///to contain all MangaRepositoryTest Unit Tests
    ///</summary>
    [TestClass]
    public abstract class MangaRepositoryTestBase
    {
        internal abstract RepositoryBase GetRepository();

        internal RepositoryBase Repository { get; set; }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        ///A test for GetChapterPagesList
        ///</summary>
        [TestMethod]
        public async Task GetChapterPagesListTest()
        {
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(Repository, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = await GetChaptersListAsync(Repository, SelectedSeries, Client);
                var SelectedChapter = PickChapter(ChaptersList);
                var PagesList = await GetPagesListAsync(Repository, SelectedChapter, Client);
                foreach (var i in PagesList)
                {
                    Assert.IsNotNull(i.PageUri);
                    Assert.AreSame(i.ParentChapter, SelectedChapter);
                    Assert.IsTrue(i.PageNo > 0);
                }
            }
        }

        /// <summary>
        ///A test for GetChaptersList
        ///</summary>
        [TestMethod]
        public async Task GetChaptersListTest()
        {
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(Repository, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = await GetChaptersListAsync(Repository, SelectedSeries, Client);
                foreach (var i in ChaptersList)
                {
                    Assert.IsNotNull(i.Title);
                    Assert.AreSame(i.ParentSeries, SelectedSeries);
                    Assert.IsNotNull(i.FirstPageUri);
                }
            }
        }

        /// <summary>
        ///A test for GetImageUri
        ///</summary>
        [TestMethod]
        public async Task GetImageUriTest()
        {
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(Repository, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = await GetChaptersListAsync(Repository, SelectedSeries, Client);
                var SelectedChapter = PickChapter(ChaptersList);
                var PagesList = await GetPagesListAsync(Repository, SelectedChapter, Client);
                var SelectedPage = PickPage(PagesList);
                string PageHTML = await Client.GetStringAsync(SelectedPage.PageUri);
                Assert.IsNotNull(PageHTML);
                SelectedPage.ImageUri = Repository.GetImageUri(PageHTML);
                Assert.IsNotNull(SelectedPage.ImageUri);
                Assert.IsTrue(System.IO.Path.HasExtension(SelectedPage.ImageUri.AbsoluteUri));
                string Extension = System.IO.Path.GetExtension(SelectedPage.ImageUri.AbsoluteUri);
                var ImageData = await Client.GetStringAsync(SelectedPage.ImageUri);
                Assert.IsNotNull(ImageData);
            }
        }

        /// <summary>
        ///A test for GetSeriesInfo
        ///</summary>
        [TestMethod]
        public async Task GetSeriesInfoTest()
        {
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(Repository, Client);
                var TestSeries = PickSeries(SeriesList);
                Assert.IsNull(TestSeries.Description);
                Assert.IsNull(TestSeries.Tags);
                string PageHTML = await Client.GetStringAsync(TestSeries.SeriesPageUri);
                Assert.IsNotNull(PageHTML);
                Repository.GetSeriesInfo(TestSeries, PageHTML);
                Assert.IsNotNull(TestSeries.Description);
                Assert.IsNotNull(TestSeries.Tags);
            }
        }

        /// <summary>
        ///A test for GetSeriesList
        ///</summary>
        [TestMethod]
        public async Task GetSeriesListTest()
        {
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(Repository, Client);
                Assert.IsNotNull(SeriesList);
                Assert.IsFalse(SeriesList.Count() == 0);
                foreach (var i in SeriesList)
                {
                    Assert.IsNotNull(i.Name);
                    Assert.AreSame(i.ParentRepository, Repository);
                    Assert.IsNotNull(i.SeriesPageUri);
                }
            }
        }

        /// <summary>
        ///A test for MangaIndexPage
        ///</summary>
        [TestMethod]
        public void MangaIndexPageTest()
        {
            Uri actual;
            actual = Repository.MangaIndexPage;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod]
        public void NameTest()
        {
            string actual;
            actual = Repository.Name;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for RootUri
        ///</summary>
        [TestMethod]
        public void RootUriTest()
        {
            Uri actual;
            actual = Repository.RootUri;
            Assert.IsNotNull(actual);
        }

        [TestInitialize]
        public void TestInitializer()
        {
            Repository = GetRepository();
        }

        internal async Task<IEnumerable<Series>> GetSeriesListAsync(RepositoryBase Repository, HttpClient Client)
        {
            string PageHTML = await Client.GetStringAsync(Repository.MangaIndexPage);
            var SeriesList = Repository.GetDefaultSeries(PageHTML);
            return SeriesList;
        }

        internal async Task<IEnumerable<Chapter>> GetChaptersListAsync(RepositoryBase Repository, Series Series, HttpClient Client)
        {
            string PageHTML = await Client.GetStringAsync(Series.SeriesPageUri);
            Assert.IsNotNull(PageHTML);
            var ChaptersList = Repository.GetChapters(Series, PageHTML);
            return ChaptersList;
        }

        internal async Task<IEnumerable<Page>> GetPagesListAsync(RepositoryBase Repository, Chapter Chapter, HttpClient Client)
        {
            var PageHTML = await Client.GetStringAsync(Chapter.FirstPageUri);
            Assert.IsNotNull(PageHTML);
            var ChaptersList = Repository.GetPages(Chapter, PageHTML);
            return ChaptersList;
        }

        internal HttpClient GetWebClient()
        {
            return new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
        }

        internal Series PickSeries(IEnumerable<Series> Input)
        {
            return Input.First();
        }

        internal Chapter PickChapter(IEnumerable<Chapter> Input)
        {
            return Input.First();
        }

        internal Page PickPage(IEnumerable<Page> Input)
        {
            return Input.First();
        }
    }
}
