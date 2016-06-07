﻿using System;
using System.IO;

namespace MangaScrapeLib.Models
{
    public class PageInfo : IPathSuggester
    {
        public readonly ChapterInfo ParentChapter;
        public readonly Uri PageUri;

        public int PageNo { get; set; }
        public Uri ImageUri { get; set; }

        public PageInfo(ChapterInfo parent, Uri pageUri)
        {
            ParentChapter = parent;
            PageUri = pageUri;
        }

        public string SuggestPath(string rootDirectoryPath)
        {
            var pathStr = String.Format("{0}{1}{2}{3}", ImageUri.Scheme, "://", ImageUri.Authority, ImageUri.AbsolutePath);
            var extension = Path.GetExtension(pathStr);
            var output = Path.Combine(ParentChapter.SuggestPath(rootDirectoryPath), SeriesInfo.MakeValidPathName(SuggestFileName()), extension);
            return output;
        }

        public string SuggestFileName()
        {
            const string numberFormatString = "000";
            var parentSeries = ParentChapter.ParentSeries;
            var output = string.Format("{0} P{1}", ParentChapter.Title, PageNo.ToString(numberFormatString));
            return output;
        }
    }
}
