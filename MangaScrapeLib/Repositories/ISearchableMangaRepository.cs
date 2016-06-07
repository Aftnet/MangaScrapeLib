using MangaScrapeLib.Models;
using System.Collections.Generic;

namespace MangaScrapeLib.Repositories
{
    public interface ISearchableMangaRepository : IMangaRepository
    {
        IEnumerable<SeriesInfo> GetSeriesFromSearch(string mangaSearchPageHtml);
    }
}
