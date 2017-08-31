using MangaScrapeLib.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    internal class Chapter : IChapter
    {
        internal Series ParentSeriesInternal { get; private set; }
        public ISeries ParentSeries => ParentSeriesInternal;

        public Uri FirstPageUri { get; private set; }
        public string Title { get; private set; }
        public string Updated { get; internal set; }

        internal Chapter(Series parent, Uri firstPageUri, string title)
        {
            ParentSeriesInternal = parent;
            FirstPageUri = firstPageUri;
            Title = title;
            Updated = string.Empty;
        }

        public virtual Task<IPage[]> GetPagesAsync()
        {
            return ParentSeriesInternal.ParentRepositoryInternal.GetPagesAsync(this);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var parentSeriesPath = ParentSeries.SuggestPath(rootDirectoryPath);
            var output = Path.Combine(parentSeriesPath, Title.MakeValidPathName());
            return output;
        }

        /*public static Chapter CreateFromData(Series parent, Uri firstPageUri, string title)
        {
            if (parent == null) throw new ArgumentNullException();
            if (firstPageUri == null) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title)) throw new ArgumentNullException();

            if (firstPageUri.Host != parent.SeriesPageUri.Host) throw new ArgumentException("Series and chapter uri mismatch");

            return new Chapter(parent, firstPageUri, title);
        }*/
    }
}
