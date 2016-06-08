using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;

namespace MangaScrapeLib.Repositories
{
    public interface ISearchableRepository : IRepository
    {
        Uri GetSearchUri(string query);
        IEnumerable<Series> GetSeriesFromSearch(string mangaSearchPageHtml);
    }
}
