using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;

namespace MangaScrapeLib.Repositories
{
    public interface IRepository
    {
        Uri MangaIndexPage { get; }
        string Name { get; }
        Uri RootUri { get; }

        IEnumerable<PageInfo> GetPages(ChapterInfo chapter, string mangaPageHtml);
        IEnumerable<ChapterInfo> GetChapters(SeriesInfo series, string seriesPageHtml);
        Uri GetImageUri(string mangaPageHtml);
        void GetSeriesInfo(SeriesInfo series, string seriesPageHtml);
        IEnumerable<SeriesInfo> GetDefaultSeries(string mangaIndexPageHtml);
    }
}