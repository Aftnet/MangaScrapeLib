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

        public abstract IEnumerable<SeriesInfo> GetSeries(string MangaIndexPageHtml);
        public abstract void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml);
        public abstract IEnumerable<ChapterInfo> GetChapters(SeriesInfo Series, string SeriesPageHtml);
        public abstract IEnumerable<PageInfo> GetPages(ChapterInfo Chapter, string MangaPageHtml);
        public abstract Uri GetImageUri(string MangaPageHtml);


        public MangaRepositoryBase(string Name, string UriString, string MangaIndexPageStr)
        {
            this.Name = Name;
            RootUri = new Uri(UriString, UriKind.Absolute);
            MangaIndexPage = new Uri(RootUri, MangaIndexPageStr);
        }

        public override string ToString()
        {
            return Name;
        }

        public static string MakeValidPathName(string Name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}]+", invalidChars);
            Name = Regex.Replace(Name, invalidReStr, " ");
            Name = Regex.Replace(Name, @"\s{2,}", " ");
            Name = Regex.Replace(Name, @"^[\s]+", "");
            Name = Regex.Replace(Name, @"[\s]+$", "");
            Name = Regex.Replace(Name, @"[\s]+", " ");
            return Name;
        }
    }
}
