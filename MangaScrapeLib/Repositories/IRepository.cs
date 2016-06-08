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

        IEnumerable<Page> GetPages(Chapter chapter, string mangaPageHtml);
        IEnumerable<Chapter> GetChapters(Series series, string seriesPageHtml);
        Uri GetImageUri(string mangaPageHtml);
        void GetSeriesInfo(Series series, string seriesPageHtml);
        IEnumerable<Series> GetDefaultSeries(string mangaIndexPageHtml);
    }
}