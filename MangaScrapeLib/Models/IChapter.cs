using System;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public interface IChapter : IPathSuggester
    {
        Uri FirstPageUri { get; }
        ISeries ParentSeries { get; }
        string Title { get; }

        Task<IPage[]> GetPagesAsync();
    }
}