using MangaScrapeLib.Models;
using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MangaScrapeLib.Test
{
    /// <summary>
    ///This is a test class for MangaRepositoryTest and is intended
    ///to contain all MangaRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public abstract class MangaRepositoryTest
    {

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
        [TestMethod()]
        public async Task GetChapterPagesListTest()
        {
            MangaRepositoryBase target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(target, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = await GetChaptersListAsync(target, SelectedSeries, Client);
                var SelectedChapter = PickChapter(ChaptersList);
                var PagesList = await GetPagesListAsync(target, SelectedChapter, Client);
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
        [TestMethod()]
        public async Task GetChaptersListTest()
        {
            MangaRepositoryBase target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(target, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = await GetChaptersListAsync(target, SelectedSeries, Client);
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
        [TestMethod()]
        public async Task GetImageUriTest()
        {
            MangaRepositoryBase target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(target, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = await GetChaptersListAsync(target, SelectedSeries, Client);
                var SelectedChapter = PickChapter(ChaptersList);
                var PagesList = await GetPagesListAsync(target, SelectedChapter, Client);
                var SelectedPage = PickPage(PagesList);
                string PageHTML = Client.DownloadString(SelectedPage.PageUri);
                Assert.IsNotNull(PageHTML);
                SelectedPage.ImageUri = target.GetImageUri(PageHTML);
                Assert.IsNotNull(SelectedPage.ImageUri);
                Assert.IsTrue(System.IO.Path.HasExtension(SelectedPage.ImageUri.AbsoluteUri));
                string Extension = System.IO.Path.GetExtension(SelectedPage.ImageUri.AbsoluteUri);
                var ImageData = Client.DownloadData(SelectedPage.ImageUri);
                Assert.IsNotNull(ImageData);
            }
        }

        /// <summary>
        ///A test for GetSeriesInfo
        ///</summary>
        [TestMethod()]
        public async Task GetSeriesInfoTest()
        {
            MangaRepositoryBase target = CreateMangaRepository(); // TODO: Initialize to an appropriate value
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(target, Client);
                var TestSeries = PickSeries(SeriesList);
                Assert.IsNull(TestSeries.Description);
                Assert.IsNull(TestSeries.Tags);
                string PageHTML = Client.DownloadString(TestSeries.SeriesPageUri);
                Assert.IsNotNull(PageHTML);
                target.GetSeriesInfo(TestSeries, PageHTML);
                Assert.IsNotNull(TestSeries.Description);
                Assert.IsNotNull(TestSeries.Tags);
            }
        }

        /// <summary>
        ///A test for GetSeriesList
        ///</summary>
        [TestMethod()]
        public async Task GetSeriesListTest()
        {
            MangaRepositoryBase target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = await GetSeriesListAsync(target, Client);
                Assert.IsNotNull(SeriesList);
                Assert.IsFalse(SeriesList.Count() == 0);
                foreach (var i in SeriesList)
                {
                    Assert.IsNotNull(i.Name);
                    Assert.AreSame(i.ParentRepository, target);
                    Assert.IsNotNull(i.SeriesPageUri);
                }
            }
        }

        /// <summary>
        ///A test for MangaIndexPage
        ///</summary>
        [TestMethod()]
        public void MangaIndexPageTest()
        {
            MangaRepositoryBase target = CreateMangaRepository();
            Uri actual;
            actual = target.MangaIndexPage;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            MangaRepositoryBase target = CreateMangaRepository();
            string actual;
            actual = target.Name;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for RootUri
        ///</summary>
        [TestMethod()]
        public void RootUriTest()
        {
            MangaRepositoryBase target = CreateMangaRepository();
            Uri actual;
            actual = target.RootUri;
            Assert.IsNotNull(actual);
        }

        internal async Task<IEnumerable<SeriesInfo>> GetSeriesListAsync(MangaRepositoryBase Repository, WebClient Client)
        {
            string PageHTML = await Client.DownloadStringTaskAsync(Repository.MangaIndexPage);
            var SeriesList = Repository.GetSeriesList(PageHTML);
            return SeriesList;
        }

        internal async Task<IEnumerable<ChapterInfo>> GetChaptersListAsync(MangaRepositoryBase Repository, SeriesInfo Series, WebClient Client)
        {
            string PageHTML = await Client.DownloadStringTaskAsync(Series.SeriesPageUri);
            Assert.IsNotNull(PageHTML);
            var ChaptersList = Repository.GetChaptersList(Series, PageHTML);
            return ChaptersList;
        }

        internal async Task<IEnumerable<PageInfo>> GetPagesListAsync(MangaRepositoryBase Repository, ChapterInfo Chapter, WebClient Client)
        {
            var PageHTML = await Client.DownloadStringTaskAsync(Chapter.FirstPageUri);
            Assert.IsNotNull(PageHTML);
            var ChaptersList = Repository.GetChapterPagesList(Chapter, PageHTML);
            return ChaptersList;
        }

        internal virtual MangaRepositoryBase CreateMangaRepository()
        {
            // TODO: Instantiate an appropriate concrete class.
            MangaRepositoryBase target = null;
            return target;
        }

        internal WebClient GetWebClient()
        {
            return new CompressionWebClient();
        }

        internal SeriesInfo PickSeries(IEnumerable<SeriesInfo> Input)
        {
            return Input.First();
        }

        internal ChapterInfo PickChapter(IEnumerable<ChapterInfo> Input)
        {
            return Input.First();
        }

        internal PageInfo PickPage(IEnumerable<PageInfo> Input)
        {
            return Input.First();
        }
    }
}
