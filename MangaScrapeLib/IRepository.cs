using System;
using System.Collections.Generic;
using System.Threading;
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
        bool SupportsLastUpdateTime { get; }
        bool SupportsTags { get; }
        bool SupportsDescription { get; }

        Task<IReadOnlyList<ISeries>> GetSeriesAsync();
        Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token);

        Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query);
        Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token);
    }
}