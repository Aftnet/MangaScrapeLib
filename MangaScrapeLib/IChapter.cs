using System;
using System.Threading.Tasks;

namespace MangaScrapeLib
{
    public interface IChapter : IPathSuggester
    {
        string Title { get; }
        string Updated { get; }

        ISeries ParentSeries { get; }
        Uri FirstPageUri { get; }

        Task<IPage[]> GetPagesAsync();
    }
}