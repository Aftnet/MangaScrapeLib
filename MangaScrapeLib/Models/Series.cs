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

        public string Name { get; internal set; }
        public Uri CoverImageUri { get; internal set; }
        public string Tags { get; internal set; }
        public string Description { get; internal set; }

        internal Series(RepositoryBase parent, Uri seriesPageUri)
        {
            ParentRepository = parent;
            SeriesPageUri = seriesPageUri;
        }

        public Task<Chapter[]> GetChaptersAsync()
        {
            return RepositoryBase.GetChaptersAsync(this);
        }

        public string SuggestPath(string rootDirectoryPath)
        {
            var output = Path.Combine(rootDirectoryPath, MakeValidPathName(Name));
            return output;
        }

        public static string MakeValidPathName(string name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);
            name = Regex.Replace(name, invalidReStr, " ");
            name = Regex.Replace(name, @"\s{2,}", " ");
            name = Regex.Replace(name, @"^[\s]+", "");
            name = Regex.Replace(name, @"[\s]+$", "");
            name = Regex.Replace(name, @"[\s]+", " ");
            return name;
        }

    }
}
