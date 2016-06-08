using MangaScrapeLib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MangaScrapeLib.Test.Models
{
    [TestClass]
    public class SeriesTest
    {
        [TestMethod]
        public void CreateFromDataWorks()
        {
            var series = Series.CreateFromData(new Uri("http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/"), "SomeTitle");
            Assert.IsNotNull(series);
        }

        [TestMethod]
        public void CreateFromDataRejectsUnsupportedUri()
        {
            var numExceptions = 0;
            try
            {
                var series = Series.CreateFromData(new Uri("http://omg.lol/"), "SomeTitle");
            }
            catch (ArgumentException)
            {
                numExceptions++;
            }

            Assert.AreEqual(1, numExceptions);
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
                    var series = Series.CreateFromData(new Uri("http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/"), i);
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
