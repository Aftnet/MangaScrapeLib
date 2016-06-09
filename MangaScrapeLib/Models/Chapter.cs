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

        protected Chapter() { }

        internal Chapter(Series parent, Uri firstPageUri, string title)
        {
            ParentSeries = parent;
            FirstPageUri = firstPageUri;
            Title = title;
        }

        public virtual Task<Page[]> GetPagesAsync()
        {
            return Repository.GetPagesAsync(this);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var parentSeriesPath = ParentSeries.SuggestPath(rootDirectoryPath);
            var output = Path.Combine(parentSeriesPath, Repository.MakeValidPathName(Title));
            return output;
        }
    }
}
