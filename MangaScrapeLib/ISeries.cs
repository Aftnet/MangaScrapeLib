using System;
using System.Collections.Generic;
using System.Threading;
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
        string Tags { get; }
        string Description { get; }

        Task<byte[]> GetCoverAsync();
        Task<byte[]> GetCoverAsync(CancellationToken token);

        Task<IReadOnlyList<IChapter>> GetChaptersAsync();
        Task<IReadOnlyList<IChapter>> GetChaptersAsync(CancellationToken token);

        IChapter GetSingleChapterFromData(Uri firstPageUri, string title, int readingOrder);
    }
}