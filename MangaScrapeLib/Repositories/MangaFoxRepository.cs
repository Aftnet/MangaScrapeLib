using HtmlAgilityPack;
using MangaScrapeLib.ExtensionMethods;
using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Repositories
{
    public class MangaFoxRepository : MangaRepositoryBase
    {
        public MangaFoxRepository() : base("Manga Fox", "http://mangafox.me/", "manga/") { }

        public override IEnumerable<SeriesInfo> GetSeries(string MangaIndexPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaIndexPageHtml);

            var Nodes = Document.DocumentNode.ChildNodes.Where(d => d.HasNameAttributeValue("div", "class", "manga_list"));
            Nodes = Nodes.SelectMany(d => d.ChildNodes.Where(e => e.Name == "a")).Where(d => d.Attributes.Contains("class")).Where(d =>
            {
                var AttrValue = d.Attributes["class"].Value;
                return (AttrValue == "series_preview manga_open" || AttrValue == "series_preview manga_close");
            });

            var Output = Nodes.Select(d => new SeriesInfo(this)
            {
                Name = WebUtility.HtmlDecode(d.InnerText),
                SeriesPageUri = new Uri(RootUri, d.Attributes["href"].Value)
            });

            Output = Output.OrderBy(d => d.Name);
            return Output;
        }

        public override void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml)
        {
            Series.Description = Series.Tags = string.Empty;
        }

        public override IEnumerable<ChapterInfo> GetChapters(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(SeriesPageHtml);

            var Nodes = Document.DocumentNode.ChildNodes.Where(d => d.HasNameAttributeValue("ul", "class", "chlist"));
            Nodes = Nodes.SelectMany(d => d.ChildNodes.Where(e => e.HasNameAttributeValue("a", "class", "tips")));

            var Output = Nodes.Select(d =>
            {
                var Chapter = new ChapterInfo(Series)
                {
                    Title = WebUtility.HtmlDecode(d.InnerText),
                    FirstPageUri = new Uri(RootUri, d.Attributes["href"].Value)
                };

                Chapter.DetectChapterNo();
                return Chapter;
            });

            Output.Reverse();
            return Output;
        }

        public override IEnumerable<PageInfo> GetPages(ChapterInfo Chapter, string MangaPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            var Nodes = Document.DocumentNode.ChildNodes.Where(d => d.HasNameAttributeValue("div", "id", "top_center_bar"));
            Nodes = Nodes.SelectMany(d => d.ChildNodes.Where(e => e.HasNameAttributeValue("div", "m", "option")));

            var Output = Nodes.Select((d, e) => new PageInfo(Chapter)
            {
                PageNo = e + 1,
                PageUri = new Uri(Regex.Replace(Chapter.FirstPageUri.AbsoluteUri, @"1\.html$", string.Format("{0}.html", d)))
            });
            return Output;
        }

        public override Uri GetImageUri(string MangaPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            var Node = Document.DocumentNode.ChildNodes.First(d => d.HasNameAttributeValue("img", "id", "image"));

            var Output = new Uri(Node.Attributes["src"].Value);
            return Output;
        }
    }
}
