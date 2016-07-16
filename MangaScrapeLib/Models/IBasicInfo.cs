using System;

namespace MangaScrapeLib.Models
{
    public interface IBasicInfo
    {
        string Title { get; }
        string Updated { get; }
    }
}
