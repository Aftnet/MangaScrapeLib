using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class Chapter : IPathSuggester
    {
        public readonly Series ParentSeries;
        public readonly Uri FirstPageUri;
        public readonly string Title;

        internal Chapter(Series parent, Uri firstPageUri, string title)
        {
            ParentSeries = parent;
            FirstPageUri = firstPageUri;
            Title = title;
        }

        public Task<Page[]> GetPagesAsync()
        {
            return Repository.GetPagesAsync(this);
        }

        public string SuggestPath(string rootDirectoryPath)
        {
            var parentSeriesPath = ParentSeries.SuggestPath(rootDirectoryPath);
            var output = Path.Combine(parentSeriesPath, Repository.MakeValidPathName(Title));
            return output;
        }
    }
}
