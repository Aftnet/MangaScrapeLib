using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class Series : IPathSuggester
    {
        public readonly RepositoryBase ParentRepository;
        public readonly Uri SeriesPageUri;
        public readonly string Name;

        public Uri CoverImageUri { get; internal set; }
        public string Tags { get; internal set; }
        public string Description { get; internal set; }

        internal Series(RepositoryBase parent, Uri seriesPageUri, string name)
        {
            ParentRepository = parent;
            SeriesPageUri = seriesPageUri;
            Name = name;
        }

        public Task<Chapter[]> GetChaptersAsync()
        {
            return RepositoryBase.GetChaptersAsync(this);
        }

        public string SuggestPath(string rootDirectoryPath)
        {
            var output = Path.Combine(rootDirectoryPath, RepositoryBase.MakeValidPathName(Name));
            return output;
        }
    }
}
