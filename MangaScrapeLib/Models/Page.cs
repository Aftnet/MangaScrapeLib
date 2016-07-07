﻿using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class Page : IPage
    {
        public IChapter ParentChapter { get; private set; }
        public Uri PageUri { get; private set; }
        public int PageNo { get; private set; }

        public Uri ImageUri { get; internal set; }

        internal Page(Chapter parent, Uri pageUri, int pageNo)
        {
            ParentChapter = parent;
            PageUri = pageUri;
            PageNo = pageNo;
        }

        public virtual Task<byte[]> GetImageAsync()
        {
            return Repository.GetImageAsync(this);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var pathStr = String.Format("{0}{1}{2}{3}", ImageUri.Scheme, "://", ImageUri.Authority, ImageUri.AbsolutePath);
            var extension = Path.GetExtension(pathStr);
            var output = Path.Combine(ParentChapter.SuggestPath(rootDirectoryPath), Repository.MakeValidPathName(SuggestFileName()), extension);
            return output;
        }

        public virtual string SuggestFileName()
        {
            const string numberFormatString = "000";
            var parentSeries = ParentChapter.ParentSeries;
            var output = string.Format("{0} P{1}", ParentChapter.Title, PageNo.ToString(numberFormatString));
            return output;
        }
    }
}
