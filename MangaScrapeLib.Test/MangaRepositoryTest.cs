using MangaScrapeLib.Models;
using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        internal virtual MangaRepository CreateMangaRepository()
        {
            // TODO: Instantiate an appropriate concrete class.
            MangaRepository target = null;
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

        /// <summary>
        ///A test for GetChapterPagesList
        ///</summary>
        [TestMethod()]
        public void GetChapterPagesListTest()
        {
            MangaRepository target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = GetSeriesList(target, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = GetChaptersList(target, SelectedSeries, Client);
                var SelectedChapter = PickChapter(ChaptersList);
                var PagesList = GetPagesList(target, SelectedChapter, Client);
                foreach (var i in PagesList)
                {
                    Assert.IsNotNull(i.PageUri);
                    Assert.AreSame(i.ParentChapter, SelectedChapter);
                    Assert.IsTrue(i.PageNo > 0);
                }
            }
        }

        internal IEnumerable<PageInfo> GetPagesList(MangaRepository Repository, ChapterInfo Chapter, WebClient Client)
        {
            string PageHTML = Client.DownloadString(Chapter.FirstPageUri);
            Assert.IsNotNull(PageHTML);
            var ChaptersList = Repository.GetChapterPagesList(Chapter, PageHTML);
            return ChaptersList;
        }

        /// <summary>
        ///A test for GetChaptersList
        ///</summary>
        [TestMethod()]
        public void GetChaptersListTest()
        {
            MangaRepository target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = GetSeriesList(target, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = GetChaptersList(target, SelectedSeries, Client);
                foreach (var i in ChaptersList)
                {
                    Assert.IsNotNull(i.Title);
                    Assert.AreSame(i.ParentSeries, SelectedSeries);
                    Assert.IsNotNull(i.FirstPageUri);
                }
            }
        }

        internal IEnumerable<ChapterInfo> GetChaptersList(MangaRepository Repository, SeriesInfo Series, WebClient Client)
        {
            string PageHTML = Client.DownloadString(Series.SeriesPageUri);
            Assert.IsNotNull(PageHTML);
            var ChaptersList = Repository.GetChaptersList(Series, PageHTML);
            return ChaptersList;
        }

        /// <summary>
        ///A test for GetImageUri
        ///</summary>
        [TestMethod()]
        public void GetImageUriTest()
        {
            MangaRepository target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = GetSeriesList(target, Client);
                var SelectedSeries = PickSeries(SeriesList);
                var ChaptersList = GetChaptersList(target, SelectedSeries, Client);
                var SelectedChapter = PickChapter(ChaptersList);
                var PagesList = GetPagesList(target, SelectedChapter, Client);
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
        public void GetSeriesInfoTest()
        {
            MangaRepository target = CreateMangaRepository(); // TODO: Initialize to an appropriate value
            using (var Client = GetWebClient())
            {
                var SeriesList = GetSeriesList(target, Client);
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
        public void GetSeriesListTest()
        {
            MangaRepository target = CreateMangaRepository();
            using (var Client = GetWebClient())
            {
                var SeriesList = GetSeriesList(target, Client);
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

        internal IEnumerable<SeriesInfo> GetSeriesList(MangaRepository Repository, WebClient Client)
        {
            string PageHTML = Client.DownloadString(Repository.MangaIndexPage);
            var SeriesList = Repository.GetSeriesList(PageHTML);
            return SeriesList;
        }

        /// <summary>
        ///A test for MangaIndexPage
        ///</summary>
        [TestMethod()]
        public void MangaIndexPageTest()
        {
            MangaRepository target = CreateMangaRepository();
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
            MangaRepository target = CreateMangaRepository();
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
            MangaRepository target = CreateMangaRepository();
            Uri actual;
            actual = target.RootUri;
            Assert.IsNotNull(actual);
        }
    }
}
