using MangaScrapeLibPortable.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MangaScrapeLibPortable.Repositories
{
    public abstract class MangaRepository
    {
        protected string name;
        public string Name { get { return name; } }

        protected Uri rootUri;
        public Uri RootUri { get { return rootUri; } }

        protected Uri mangaIndexPage;
        public Uri MangaIndexPage { get { return mangaIndexPage; } }

        public MangaRepository(string Name, string UriString, string MangaIndexPageStr)
        {
            name = Name;
            rootUri = new Uri(UriString, UriKind.Absolute);
            mangaIndexPage = new Uri(rootUri, MangaIndexPageStr);
        }

        public static List<MangaRepository> GetAllRepositories()
        {
            var Output = new List<MangaRepository>()
            {
                new EatMangaRepository(),
                new MangaFoxRepository(),
                new MangaHereRepository()
            };
            return Output;
        }

        public abstract IEnumerable<SeriesInfo> GetSeriesList(string MangaIndexPageHtml);
        public abstract void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml);
        public abstract IEnumerable<ChapterInfo> GetChaptersList(SeriesInfo Series, string SeriesPageHtml);
        public abstract IEnumerable<PageInfo> GetChapterPagesList(ChapterInfo Chapter, string MangaPageHtml);
        public abstract Uri GetImageUri(string MangaPageHtml);

        /*protected static HtmlAgilityPack.HtmlNode GetElement(string PageHtml, string XPath)
        {
            HtmlDocument PageDocument = new HtmlDocument();
            PageDocument.LoadHtml(PageHtml);
            var SelectedNode = PageDocument.SelectSingleNode(XPath);

            return SelectedNode;
        }

        protected static HtmlAgilityPack.HtmlNodeCollection GetElements(string PageHtml, string XPath)
        {
            HtmlDocument PageDocument = new HtmlDocument();
            PageDocument.LoadHtml(PageHtml);
            var SelectedNode = PageDocument.DocumentNode.SelectNodes(XPath);

            return SelectedNode;
        }

        protected static string GetElementAttributeVal(string PageHtml, string XPath, string AttributeName)
        {
            var SelectedNode = GetElement(PageHtml, XPath);

            if (SelectedNode == null) return null;
            if (SelectedNode.Attributes[AttributeName] == null) return null;

            return SelectedNode.Attributes[AttributeName].Value;
        }

        protected Uri GetElementAttributeUri(string PageHtml, string XPath, string AttributeName)
        {
            string AttributeVal = GetElementAttributeVal(PageHtml, XPath, AttributeName);
            if (AttributeVal == null) return null;
            return new Uri(RootUri, AttributeVal);
        }*/

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
