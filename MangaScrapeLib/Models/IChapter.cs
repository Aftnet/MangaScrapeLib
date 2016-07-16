using System;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public interface IChapter : IBasicInfo, IPathSuggester
    {
        ISeries ParentSeries { get; }
        Uri FirstPageUri { get; }

        Task<IPage[]> GetPagesAsync();
    }
}