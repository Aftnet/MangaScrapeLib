using System;
using System.Threading.Tasks;

namespace MangaScrapeLib
{
    public interface IRepository
    {
        byte[] Icon { get; }
        string Name { get; }
        Uri RootUri { get; }
        SeriesMetadataSupport SeriesMetadata { get; }

        Task<ISeries[]> GetSeriesAsync();
        Task<ISeries[]> SearchSeriesAsync(string query);
    }
}