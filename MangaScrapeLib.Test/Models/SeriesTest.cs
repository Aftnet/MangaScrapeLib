using MangaScrapeLib.Models;
using System;
using Xunit;

namespace MangaScrapeLib.Test.Models
{
    public class SeriesTest
    {
        /*public static readonly Uri ValidSeriesUri = new Uri("http://eatmanga.com/Manga-Scan/Yamada-kun-to-7-nin-no-Majo/");

        [Fact]
        public void CreateFromDataWorks()
        {
            var series = Series.CreateFromData(ValidSeriesUri, "SomeTitle");
            Assert.NotNull(series);
        }

        [Fact]
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

            Assert.Equal(invalidUris.Length, numExceptions);
        }

        [Fact]
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

            Assert.Equal(invalidTitles.Length, numExceptions);
        }*/
    }
}
