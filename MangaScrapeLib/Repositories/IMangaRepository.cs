﻿using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;

namespace MangaScrapeLib.Repositories
{
    public interface IMangaRepository
    {
        Uri MangaIndexPage { get; }
        string Name { get; }
        Uri RootUri { get; }

        IEnumerable<PageInfo> GetPages(ChapterInfo Chapter, string MangaPageHtml);
        IEnumerable<ChapterInfo> GetChapters(SeriesInfo Series, string SeriesPageHtml);
        Uri GetImageUri(string MangaPageHtml);
        void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml);
        IEnumerable<SeriesInfo> GetSeries(string MangaIndexPageHtml);
    }
}