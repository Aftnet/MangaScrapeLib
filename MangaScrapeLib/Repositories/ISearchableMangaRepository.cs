using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;

namespace MangaScrapeLib.Repositories
{
    public interface ISearchableMangaRepository : IMangaRepository
    {
        Uri GetSearchUri(string query);
        IEnumerable<SeriesInfo> GetSeriesFromSearch(string mangaSearchPageHtml);
    }
}
