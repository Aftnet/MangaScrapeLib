using System;
using System.IO;

namespace MangaScrapeLib.Models
{
    public class ChapterInfo : IPathSuggester
    {
        public readonly SeriesInfo ParentSeries;

        public readonly Uri FirstPageUri;

        public string Title { get; set; }

        public ChapterInfo(SeriesInfo parent, Uri firstPageUri)
        {
            ParentSeries = parent;
            FirstPageUri = firstPageUri;
        }

        public string SuggestPath(string rootDirectoryPath)
        {
            var parentSeriesPath = ParentSeries.SuggestPath(rootDirectoryPath);
            var output = Path.Combine(ParentSeries.SuggestPath(rootDirectoryPath), SeriesInfo.MakeValidPathName(Title));

            output = SeriesInfo.MakeValidPathName(output);
            return output;
        }
    }
}
