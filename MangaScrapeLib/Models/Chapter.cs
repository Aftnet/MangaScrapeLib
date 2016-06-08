using System;
using System.IO;
using System.Threading.Tasks;

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

        public Task<Page[]> GetPagesAsync()
        {
            return Source.GetPagesAsync(this);
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
