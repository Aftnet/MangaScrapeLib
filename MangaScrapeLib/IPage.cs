using System;
using System.Threading.Tasks;

namespace MangaScrapeLib
{
    public interface IPage : IPathSuggester
    {
        Uri ImageUri { get; }
        int PageNo { get; }
        Uri PageUri { get; }
        IChapter ParentChapter { get; }

        Task<byte[]> GetImageAsync();
        string SuggestFileName();
    }
}