using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories.Components
{
    internal interface ISearcher
    {
        Task<ISeries[]> GetSeriesAsync();
        Task<ISeries[]> SearchSeriesAsync(string query);
    }
}
