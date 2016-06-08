using MangaScrapeLib.Models;
using System;

namespace MangaScrapeLib.Repositories
{
    internal interface IRepository
    {
        Uri MangaIndexPage { get; }
        string Name { get; }
        Uri RootUri { get; }

        Page[] GetPages(Chapter chapter, string mangaPageHtml);
        Chapter[] GetChapters(Series series, string seriesPageHtml);
        Uri GetImageUri(string mangaPageHtml);
        void GetSeriesInfo(Series series, string seriesPageHtml);
        Series[] GetDefaultSeries(Source source, string mangaIndexPageHtml);
    }
}