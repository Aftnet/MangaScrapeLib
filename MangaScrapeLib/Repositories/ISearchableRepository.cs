using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;

namespace MangaScrapeLib.Repositories
{
    public interface ISearchableRepository : IRepository
    {
        Uri GetSearchUri(string query);
        IEnumerable<SeriesInfo> GetSeriesFromSearch(string mangaSearchPageHtml);
    }
}
