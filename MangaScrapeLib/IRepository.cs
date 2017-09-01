using System;
using System.Threading.Tasks;

namespace MangaScrapeLib
{
    public interface IRepository
    {
        byte[] Icon { get; }
        string Name { get; }
        Uri RootUri { get; }

        bool SupportsCover { get; }
        bool SupportsAuthor { get; }
        bool SupportsReleaseTime { get; }
        bool SupportsTags { get; }
        bool SupportsDescription { get; }

        Task<ISeries[]> GetSeriesAsync();
        Task<ISeries[]> SearchSeriesAsync(string query);

        ISeries GetSingleSeriesFromData(Uri seriesPageUri, string name);
    }
}