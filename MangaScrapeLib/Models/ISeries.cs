using System;
using System.Threading.Tasks;
using MangaScrapeLib.Repositories;

namespace MangaScrapeLib.Models
{
    public interface ISeries : IBasicInfo, IPathSuggester
    {
        IRepository ParentRepository { get; }
        Uri SeriesPageUri { get; }
        Uri CoverImageUri { get; }
        string Description { get; }

        Task<IChapter[]> GetChaptersAsync();
    }
}