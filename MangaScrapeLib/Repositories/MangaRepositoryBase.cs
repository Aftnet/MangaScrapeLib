using AngleSharp.Parser.Html;
using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    public abstract class MangaRepositoryBase : IMangaRepository
    {
        protected static readonly HtmlParser Parser = new HtmlParser();

        public string Name { get; private set; }
        public Uri RootUri { get; private set; }
        public Uri MangaIndexPage { get; private set; }

        public abstract IEnumerable<SeriesInfo> GetSeries(string mangaIndexPageHtml);
        public abstract void GetSeriesInfo(SeriesInfo series, string seriesPageHtml);
        public abstract IEnumerable<ChapterInfo> GetChapters(SeriesInfo series, string seriesPageHtml);
        public abstract IEnumerable<PageInfo> GetPages(ChapterInfo chapter, string mangaPageHtml);
        public abstract Uri GetImageUri(string mangaPageHtml);


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
