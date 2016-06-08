using System;
using System.IO;

namespace MangaScrapeLib.Models
{
    public class Chapter : IPathSuggester
    {
        public readonly Series ParentSeries;

        public readonly Uri FirstPageUri;

        public string Title { get; internal set; }

        internal Chapter(Series parent, Uri firstPageUri)
        {
            ParentSeries = parent;
            FirstPageUri = firstPageUri;
        }

        public string SuggestPath(string rootDirectoryPath)
        {
            var parentSeriesPath = ParentSeries.SuggestPath(rootDirectoryPath);
            var output = Path.Combine(ParentSeries.SuggestPath(rootDirectoryPath), Series.MakeValidPathName(Title));

            output = Series.MakeValidPathName(output);
            return output;
        }
    }
}
