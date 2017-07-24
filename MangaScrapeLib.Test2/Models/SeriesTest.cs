using MangaScrapeLib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MangaScrapeLib.Test.Models
{
    [TestClass]
    public class SeriesTest
    {
        public static readonly Uri ValidSeriesUri = new Uri("http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/");

        [TestMethod]
        public void CreateFromDataWorks()
        {
            var series = Series.CreateFromData(ValidSeriesUri, "SomeTitle");
            Assert.IsNotNull(series);
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
                    var series = Series.CreateFromData(i, "SomeTitle");
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
                    var series = Series.CreateFromData(ValidSeriesUri, i);
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
