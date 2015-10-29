using System;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Models
{
    public class ChapterInfo : IPathSuggester
    {
        public SeriesInfo ParentSeries { get; set; }

        public int ChapterNo { get; set; }
        public string Title { get; set; }
        public Uri FirstPageUri { get; set; }

        public void DetectChapterNo()
        {
            var SeriesNameLowercase = ParentSeries.Name.ToLowerInvariant();
            var ChapterTitleNoRegex = new Regex(string.Format(@"{0} [\d]+$|{0} [\d]+ ", SeriesNameLowercase));

            //Try to detect chapter number
            ChapterNo = -1;
            var TitleLowercase = Title.ToLowerInvariant();
            var Match = ChapterTitleNoRegex.Match(TitleLowercase);
            if (Match.Success == true)
            {
                string NumStr = Match.Value.Substring(SeriesNameLowercase.Length + 1);
                ChapterNo = int.Parse(NumStr);
            }
        }

        public string SuggestPath(string RootDirectoryPath)
        {
            var ParentSeriesPath = ParentSeries.SuggestPath(RootDirectoryPath);
            var Output = string.Format("{0}{1}{2} {3}", ParentSeriesPath, SeriesInfo.PathSeparator, ParentSeries.Name, ChapterNo.ToString("000"));
            if (ChapterNo < 0)
            {
                Output = string.Format("{0}{2}Extra{2}{1}", ParentSeriesPath, Title, SeriesInfo.PathSeparator);
            }

            Output = SeriesInfo.MakeValidPathName(Output);
            return Output;
        }

    }
}
