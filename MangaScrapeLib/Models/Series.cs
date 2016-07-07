using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class Series : IPathSuggester
    {
        public readonly Repository ParentRepository;
        public readonly Uri SeriesPageUri;
        public readonly string Name;

        public Uri CoverImageUri { get; internal set; }
        public string Tags { get; internal set; }
        public string Description { get; internal set; }

        internal Series(Repository parent, Uri seriesPageUri, string name)
        {
            ParentRepository = parent;
            SeriesPageUri = seriesPageUri;
            Name = name;
        }

        public virtual Task<Chapter[]> GetChaptersAsync()
        {
            return Repository.GetChaptersAsync(this);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var output = Path.Combine(rootDirectoryPath, Repository.MakeValidPathName(Name));
            return output;
        }

        public static Series CreateFromData(Uri seriesPageUri, string name)
        {
            if (seriesPageUri == null) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException();

            var allRepositories = Repository.AllRepositories;
            var repository = allRepositories.FirstOrDefault(d => d.RootUri.Host == seriesPageUri.Host);
            if (repository == null) throw new ArgumentException("Series page Uri does not match any supported repository");

            return new Series(repository, seriesPageUri, name);
        }
    }
}
