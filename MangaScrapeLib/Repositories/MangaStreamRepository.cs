using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaScrapeLib.Models;

namespace MangaScrapeLib.Repositories
{
    class MangaStreamRepository : Repository
    {
        public MangaStreamRepository() : base("Mangastream", "http://mangastream.com/", "manga/", "MangaStream.png")
        {
        }

        internal override Series[] GetDefaultSeries(string mangaIndexPageHtml)
        {
            throw new NotImplementedException();
        }

        internal override Chapter[] GetChapters(Series series, string seriesPageHtml)
        {
            throw new NotImplementedException();
        }

        internal override Uri GetImageUri(string mangaPageHtml)
        {
            throw new NotImplementedException();
        }

        internal override Page[] GetPages(Chapter chapter, string mangaPageHtml)
        {
            throw new NotImplementedException();
        }

        internal override void GetSeriesInfo(Series series, string seriesPageHtml)
        {
            throw new NotImplementedException();
        }
    }
}
