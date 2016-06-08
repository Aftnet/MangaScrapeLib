using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Models
{
    public class Series : IPathSuggester
    {
        public readonly IRepository ParentRepository;
        public readonly Uri SeriesPageUri;

        public string Name { get; set; }

        public string Tags { get; set; }
        public string Description { get; set; }

        public Series(IRepository parent, Uri seriesPageUri)
        {
            ParentRepository = parent;
            SeriesPageUri = seriesPageUri;
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
