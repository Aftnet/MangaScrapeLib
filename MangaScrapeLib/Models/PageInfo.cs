using MangaScrapeLib.Repositories;
using System;
using System.IO;

namespace MangaScrapeLib.Models
{
    public class PageInfo : IPathSuggester
    {
        public ChapterInfo ParentChapter { get; set; }

        public int PageNo { get; set; }
        public Uri PageUri { get; set; }
        public Uri ImageUri { get; set; }
        public byte[] ImageData { get; set; }

        public string SuggestFileName()
        {
            SeriesInfo ParentSeries = ParentChapter.ParentSeries;

            var Output = string.Format("{0} C{1} P{2}", ParentSeries.Name, ParentChapter.ChapterNo.ToString("000"), PageNo.ToString("000"));
            if (ParentChapter.ChapterNo < 0)
            {
                Output = string.Format("{0} P{1}", ParentChapter.Title, PageNo.ToString("000"));
            }

            return Output;
        }

        public string SuggestPath(string RootDirectoryPath)
        {
            var PathStr = String.Format("{0}{1}{2}{3}", ImageUri.Scheme, "://", ImageUri.Authority, ImageUri.AbsolutePath);
            var Extension = Path.GetExtension(PathStr);
            var Output = string.Format("{0}{1}{2}{3}", ParentChapter.SuggestPath(RootDirectoryPath), SeriesInfo.PathSeparator, SuggestFileName(), Extension);
            Output = SeriesInfo.MakeValidPathName(Output);
            return Output;
        }
    }
}
