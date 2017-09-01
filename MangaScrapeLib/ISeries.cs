using System;
using System.Threading.Tasks;

namespace MangaScrapeLib
{
    public interface ISeries : IPathSuggester
    {
        IRepository ParentRepository { get; }

        string Title { get; }
        string Updated { get; }

        Uri SeriesPageUri { get; }
        Uri CoverImageUri { get; }
        string Author { get; }
        string Release { get; }
        string Tags { get; }
        string Description { get; }

        Task<IChapter[]> GetChaptersAsync();

        IChapter GetSingleChapterFromData(Uri firstPageUri, string title);
    }
}