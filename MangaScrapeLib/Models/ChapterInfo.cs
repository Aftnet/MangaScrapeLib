using System;
using System.Text.RegularExpressions;

namespace MangaScrapeLibPortable.Models
{
    public class ChapterInfo
    {
        public SeriesInfo ParentSeries { get; set; }

        public int ChapterNo { get; set; }
        public string Title { get; set; }
        public Uri FirstPageUri { get; set; }

        public void DetectChapterNo()
        {
            string SeriesNameLowercase = ParentSeries.Name.ToLowerInvariant();
            Regex ChapterTitleNoRegex = new Regex(string.Format(@"{0} [\d]+$|{0} [\d]+ ", SeriesNameLowercase));

            //Try to detect chapter number
            ChapterNo = -1;
            string TitleLowercase = Title.ToLowerInvariant();
            var Match = ChapterTitleNoRegex.Match(TitleLowercase);
            if (Match.Success == true)
            {
                string NumStr = Match.Value.Substring(SeriesNameLowercase.Length + 1);
                ChapterNo = int.Parse(NumStr);
            }
        }
    }
}
