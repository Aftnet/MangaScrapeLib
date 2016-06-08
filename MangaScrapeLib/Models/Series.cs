using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Models
{
    public class Series : IPathSuggester
    {
        internal readonly IRepository ParentRepository;
        public readonly Uri SeriesPageUri;

        public string Name { get; internal set; }
        public Uri CoverImageUri { get; internal set; }
        public string Tags { get; internal set; }
        public string Description { get; internal set; }

        internal Series(IRepository parent, Uri seriesPageUri)
        {
            ParentRepository = parent;
            SeriesPageUri = seriesPageUri;
        }

        public static Series GetFromUri(Uri seriesPageUri)
        {
            var matchingSource = Source.AllSources.Where(d => d.Repository.MangaIndexPage.Host == seriesPageUri.Host).FirstOrDefault();
            if (matchingSource == null) throw new ArgumentException();

            return new Series(matchingSource.Repository, seriesPageUri);
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
