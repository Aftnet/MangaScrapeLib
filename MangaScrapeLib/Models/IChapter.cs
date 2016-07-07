using System;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public interface IChapter : IPathSuggester
    {
        Uri FirstPageUri { get; }
        Series ParentSeries { get; }
        string Title { get; }

        Task<IPage[]> GetPagesAsync();
    }
}