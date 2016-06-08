using MangaScrapeLib.Models;
using System;

namespace MangaScrapeLib.Repositories
{
    internal interface ISearchableRepository : IRepository
    {
        Uri GetSearchUri(string query);
        Series[] GetSeriesFromSearch(Source source, string mangaSearchPageHtml);
    }
}
