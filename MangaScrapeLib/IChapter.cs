using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib
{
    public interface IChapter : IPathSuggester
    {
        string Title { get; }
        int ReadingOrder { get; }
        string Updated { get; }

        ISeries ParentSeries { get; }
        Uri FirstPageUri { get; }

        Task<IReadOnlyList<IPage>> GetPagesAsync();
        Task<IReadOnlyList<IPage>> GetPagesAsync(CancellationToken token);
    }
}