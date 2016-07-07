using System;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public interface IPage : IPathSuggester
    {
        Uri ImageUri { get; }
        int PageNo { get; }
        Uri PageUri { get; }
        Chapter ParentChapter { get; }

        Task<byte[]> GetImageAsync();
        string SuggestFileName();
    }
}