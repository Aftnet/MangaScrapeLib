using System;
using System.IO;

namespace MangaScrapeLib.Models
{
    public class ChapterInfo : IPathSuggester
    {
        public readonly SeriesInfo ParentSeries;

        public readonly Uri FirstPageUri;

        public string Title { get; set; }

        public ChapterInfo(SeriesInfo Parent, Uri FirstPageUri)
        {
            ParentSeries = Parent;
            this.FirstPageUri = FirstPageUri;
        }

        public string SuggestPath(string RootDirectoryPath)
        {
            var ParentSeriesPath = ParentSeries.SuggestPath(RootDirectoryPath);
            var Output = Path.Combine(ParentSeries.SuggestPath(RootDirectoryPath), SeriesInfo.MakeValidPathName(Title));

            Output = SeriesInfo.MakeValidPathName(Output);
            return Output;
        }
    }
}
