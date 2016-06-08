﻿using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class Page : IPathSuggester
    {
        public readonly Chapter ParentChapter;
        public readonly Uri PageUri;
        public readonly int PageNo;

        public Uri ImageUri { get; internal set; }

        internal Page(Chapter parent, Uri pageUri, int pageNo)
        {
            ParentChapter = parent;
            PageUri = pageUri;
            PageNo = pageNo;
        }

        public Task<byte[]> GetImageAsync()
        {
            return RepositoryBase.GetImageAsync(this);
        }

        public string SuggestPath(string rootDirectoryPath)
        {
            var pathStr = String.Format("{0}{1}{2}{3}", ImageUri.Scheme, "://", ImageUri.Authority, ImageUri.AbsolutePath);
            var extension = Path.GetExtension(pathStr);
            var output = Path.Combine(ParentChapter.SuggestPath(rootDirectoryPath), RepositoryBase.MakeValidPathName(SuggestFileName()), extension);
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