using MangaScrapeLib.Repositories;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Models
{
    public class SeriesInfo
    {
        public const string PathSeparator = @"\";

        public IMangaRepository ParentRepository { get; set; }

        public string Name { get; set; }
        public Uri SeriesPageUri { get; set; }

        public string Tags { get; set; }
        public string Description { get; set; }

        public string SuggestPath(string RootDirectoryPath)
        {
            var Output = string.Format("{0}{1}{2}", RootDirectoryPath, PathSeparator, Name);
            Output = MakeValidPathName(Output);
            return Output;
        }

        public static string MakeValidPathName(string Name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);
            Name = Regex.Replace(Name, invalidReStr, " ");
            Name = Regex.Replace(Name, @"\s{2,}", " ");
            Name = Regex.Replace(Name, @"^[\s]+", "");
            Name = Regex.Replace(Name, @"[\s]+$", "");
            Name = Regex.Replace(Name, @"[\s]+", " ");
            return Name;
        }

    }
}
