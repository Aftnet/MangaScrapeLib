using System;
using System.Threading.Tasks;
using MangaScrapeLib.Repositories;

namespace MangaScrapeLib.Models
{
    public interface ISeries : IPathSuggester
    {
        Uri CoverImageUri { get; }
        string Description { get; }
        string Title { get; }
        IRepository ParentRepository { get; }
        Uri SeriesPageUri { get; }

        Task<IChapter[]> GetChaptersAsync();
    }
}