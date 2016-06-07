using System;
using System.IO;

namespace MangaScrapeLib.Models
{
    public class PageInfo : IPathSuggester
    {
        public readonly ChapterInfo ParentChapter;
        public readonly Uri PageUri;

        public int PageNo { get; set; }
        public Uri ImageUri { get; set; }

        public PageInfo(ChapterInfo Parent, Uri PageUri)
        {
            ParentChapter = Parent;
            this.PageUri = PageUri;
        }

        public string SuggestPath(string RootDirectoryPath)
        {
            var PathStr = String.Format("{0}{1}{2}{3}", ImageUri.Scheme, "://", ImageUri.Authority, ImageUri.AbsolutePath);
            var Extension = Path.GetExtension(PathStr);
            var Output = Path.Combine(ParentChapter.SuggestPath(RootDirectoryPath), SeriesInfo.MakeValidPathName(SuggestFileName()), Extension);
            return Output;
        }

        public string SuggestFileName()
        {
            const string NumberFormatString = "000";
            SeriesInfo ParentSeries = ParentChapter.ParentSeries;
            var Output = string.Format("{0} P{1}", ParentChapter.Title, PageNo.ToString(NumberFormatString));
            return Output;
        }
    }
}
