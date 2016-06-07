using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Repositories
{
    public abstract class MangaRepositoryBase
    {
        protected static readonly HtmlParser Parser = new HtmlParser();

        public string Name { get; private set; }
        public Uri RootUri { get; private set; }
        public Uri MangaIndexPage { get; private set; }

        public MangaRepositoryBase(string name, string uriString, string mangaIndexPageStr)
        {
            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);
            MangaIndexPage = new Uri(RootUri, mangaIndexPageStr);
        }

        public override string ToString()
        {
            return Name;
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
