using System;
using System.Collections.Generic;
using MangaScrapeLib.Models;

namespace MangaScrapeLib.Repositories
{
    public interface IMangaRepository
    {
        Uri MangaIndexPage { get; }
        string Name { get; }
        Uri RootUri { get; }

        IEnumerable<PageInfo> GetChapterPagesList(ChapterInfo Chapter, string MangaPageHtml);
        IEnumerable<ChapterInfo> GetChaptersList(SeriesInfo Series, string SeriesPageHtml);
        Uri GetImageUri(string MangaPageHtml);
        void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml);
        IEnumerable<SeriesInfo> GetSeriesList(string MangaIndexPageHtml);
    }
}