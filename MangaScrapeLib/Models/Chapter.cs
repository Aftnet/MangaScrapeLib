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

        public static Chapter CreateFromData(Series parent, Uri firstPageUri, string title)
        {
            if (parent == null) throw new ArgumentNullException();
            if (firstPageUri == null) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title)) throw new ArgumentNullException();

            if (firstPageUri.Host != parent.SeriesPageUri.Host) throw new ArgumentException("Series and chapter uri mismatch");

            return new Chapter(parent, firstPageUri, title);
        }
    }
}
