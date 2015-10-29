using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class ChapterInfo : IPathSuggester
    {
        public readonly SeriesInfo ParentSeries;

        public int ChapterNo { get; set; }
        public string Title { get; set; }
        public Uri FirstPageUri { get; set; }

        public ChapterInfo(SeriesInfo Parent)
        {
            ParentSeries = Parent;
        }

        public async Task<IEnumerable<PageInfo>> GetPagesAsync(bool ParseHtmlAsynchronously = true)
        {
            using (var Client = new HttpClient())
            {
                var PageHtml = await Client.GetStringAsync(FirstPageUri);
                if (ParseHtmlAsynchronously)
                {
                    return await Task.Run(() => ParentSeries.ParentRepository.GetPages(this, PageHtml));
                }

                var Output = ParentSeries.ParentRepository.GetPages(this, PageHtml);
                return Output;
            }
        }
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
