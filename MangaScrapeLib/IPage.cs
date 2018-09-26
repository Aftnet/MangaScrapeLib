using System;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib
{
    public interface IPage : IPathSuggester
    {
        Uri ImageUri { get; }
        int PageNumber { get; }
        Uri PageUri { get; }
        IChapter ParentChapter { get; }

        Task<byte[]> GetImageAsync();
        Task<byte[]> GetImageAsync(CancellationToken token);

        string SuggestFileName();
    }
}