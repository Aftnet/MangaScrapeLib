using MangaScrapeLib.Repositories;
using MangaScrapeLib.Tools;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    internal class Series : ISeries
    {
        public RepositoryBase ParentRepositoryInternal { get; private set; }
        public IRepository ParentRepository => ParentRepositoryInternal;

        public Uri SeriesPageUri { get; private set; }
        public string Title { get; private set; }
        public string Updated { get; internal set; }

        public Uri CoverImageUri { get; internal set; }
        public string Author { get; internal set; }
        public string Release { get; internal set; }
        public string Tags { get; internal set; }
        public string Description { get; internal set; }

        internal Series(RepositoryBase parent, Uri seriesPageUri, string name)
        {
            ParentRepositoryInternal = parent;
            SeriesPageUri = seriesPageUri;
            Title = name;
            Updated = string.Empty;
        }

        public virtual Task<IChapter[]> GetChaptersAsync()
        {
            return ParentRepositoryInternal.GetChaptersAsync(this);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var output = Path.Combine(rootDirectoryPath, Title.MakeValidPathName());
            return output;
        }

        /*public static Series CreateFromData(Uri seriesPageUri, string name)
        {
            if (seriesPageUri == null) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException();

            var allRepositories = Repository.AllRepositories;
            var repository = allRepositories.FirstOrDefault(d => d.RootUri.Host == seriesPageUri.Host);
            if (repository == null) throw new ArgumentException("Series page Uri does not match any supported repository");

            return new Series(repository, seriesPageUri, name);
        }*/
    }
}
