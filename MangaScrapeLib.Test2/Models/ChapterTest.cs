using MangaScrapeLib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MangaScrapeLib.Test.Models
{
    [TestClass]
    public class ChapterTest
    {
        public static readonly Uri ValidChapterUri = new Uri("http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/testch/");

        protected Series TestSeries = Series.CreateFromData(SeriesTest.ValidSeriesUri, "SomeTitle");

        [TestMethod]
        public void CreateFromDataWorks()
        {
            var chapter = Chapter.CreateFromData(TestSeries, ValidChapterUri, "Test chapter");
            Assert.IsNotNull(chapter);
        }

        [TestMethod]
        public void CreateFromDataRejectsNullSeries()
        {
            var numExceptions = 0;
            try
            {
                var chapter = Chapter.CreateFromData(null, ValidChapterUri, "Test chapter");
            }
            catch (ArgumentException)
            {
                numExceptions++;
            }

            Assert.AreEqual(1, numExceptions);
        }

        [TestMethod]
        public void CreateFromDataRejectsUnsupportedUri()
        {
            var invalidUris = new Uri[] { null, new Uri("http://omg.lol/") };

            var numExceptions = 0;
            foreach (var i in invalidUris)
            {
                try
                {
                    var chapter = Chapter.CreateFromData(TestSeries, i, "SomeTitle");
                }
                catch (ArgumentException)
                {
                    numExceptions++;
                }
            }

            Assert.AreEqual(invalidUris.Length, numExceptions);
        }

        [TestMethod]
        public void CreateFromDataRejectsInvalidTitle()
        {
            var invalidTitles = new string[] { null, string.Empty, "  " };
            var numExceptions = 0;
            foreach (var i in invalidTitles)
            {
                try
                {
                    var chapter = Chapter.CreateFromData(TestSeries, ValidChapterUri, i);
                }
                catch (ArgumentNullException)
                {
                    numExceptions++;
                }
            }

            Assert.AreEqual(invalidTitles.Length, numExceptions);
        }
    }
}
