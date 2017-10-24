using System;
using System.Threading.Tasks;
using MangaScrapeLib.Models;

namespace MangaScrapeLib.Repositories
{
    public interface IRepository
    {
        byte[] Icon { get; }
        Uri MangaIndexPage { get; }
        string Name { get; }
        Uri RootUri { get; }
        SeriesMetadataSupport SeriesMetadata { get; }

        Task<ISeries[]> GetSeriesAsync();
        Task<ISeries[]> SearchSeriesAsync(string query);
    }
}