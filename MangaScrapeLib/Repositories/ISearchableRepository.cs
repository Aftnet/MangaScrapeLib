using MangaScrapeLib.Models;
using System;

namespace MangaScrapeLib.Repositories
{
    public interface ISearchableRepository : IRepository
    {
        Uri GetSearchUri(string query);
        Series[] GetSeriesFromSearch(string mangaSearchPageHtml);
    }
}
