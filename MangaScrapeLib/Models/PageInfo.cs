using MangaScrapeLib.Repositories;
using System;

namespace MangaScrapeLib.Models
{
    public class PageInfo
    {
        public ChapterInfo ParentChapter { get; set; }

        public int PageNo { get; set; }
        public Uri PageUri { get; set; }
        public Uri ImageUri { get; set; }
        public byte[] ImageData { get; set; }

        public string SuggestFileName()
        {
            SeriesInfo ParentSeries = ParentChapter.ParentSeries;

            string Output = null;
            if (ParentChapter.ChapterNo >= 0)
            {
                Output = string.Format("{0} C{1} P{2}", MangaRepositoryBase.MakeValidPathName(ParentSeries.Name), ParentChapter.ChapterNo.ToString("000"), PageNo.ToString("000"));
            }
            else
            {
                Output = string.Format("{0} P{1}", MangaRepositoryBase.MakeValidPathName(ParentChapter.Title), PageNo.ToString("000"));
            }

            return Output;
        }
    }
}
