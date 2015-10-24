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
    public class MangaReaderRepository : MangaRepositoryBase
    {
        public MangaReaderRepository() : base("Manga Reader", "http://www.mangareader.net/", "alphabetical") { }

        public override IEnumerable<SeriesInfo> GetSeriesList(string MangaIndexPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaIndexPageHtml);

            var Nodes = Document.DocumentNode.Descendants().Where(d => d.HasNameAttributeValue("ul", "class", "series_alpha"));
            Nodes = Nodes.SelectMany(d => d.ChildNodes.Where(e => e.Name == "li")).Select(d => d.ChildNodes.First(e => e.Name == "a"));
            var Output = Nodes.Select(d => new SeriesInfo()
            {
                ParentRepository = this,
                Name = WebUtility.HtmlDecode(d.InnerText),
                SeriesPageUri = new Uri(RootUri, d.Attributes["href"].Value)
            });
            Output = Output.OrderBy(d => d.Name);
            return Output;
        }

        public override void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(SeriesPageHtml);

            var TopTextNode = Document.DocumentNode.ChildNodes.First(d => d.HasNameAttributeValue("ul", "class", "detail_topText"));
            var Node = TopTextNode.ChildNodes.Where(d => d.Name == "li").ElementAt(3);
            Node.RemoveChild(Node.FirstChild);
            Series.Tags = Node.InnerText;

            Node = TopTextNode.ChildNodes.Where(d => d.Name == "li").ElementAt(7).ChildNodes.Where(d => d.Name == "p").ElementAt(1);
            Node.RemoveChild(Node.ChildNodes[1]);
            Series.Description = WebUtility.HtmlDecode(Node.InnerText);
        }

        public override IEnumerable<ChapterInfo> GetChaptersList(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(SeriesPageHtml);

            var Node = Document.DocumentNode.ChildNodes.First(d => d.HasNameAttributeValue("div", "class", "manga_detail"));
            Node = Node.ChildNodes.First(d => d.HasNameAttributeValue("div", "class", "detail_list"));
            Node = Node.ChildNodes.First(d => d.Name == "ul");
            var Nodes = Node.ChildNodes.Where(d => d.HasNameAttributeValue("a", "class", "color_0077"));

            var Output = Nodes.Select(d =>
            {
                string Title = d.InnerText;
                Title = Regex.Replace(Title, @"^[\r\n\s\t]+", string.Empty);
                Title = Regex.Replace(Title, @"[\r\n\s\t]+$", string.Empty);
                var Chapter = new ChapterInfo()
                {
                    ParentSeries = Series,
                    Title = Title,
                    FirstPageUri = new Uri(RootUri, d.Attributes["href"].Value)
                };

                Chapter.DetectChapterNo();
                return Chapter;
            });

            Output.Reverse();
            return Output;
        }

        public override IEnumerable<PageInfo> GetChapterPagesList(ChapterInfo Chapter, string MangaPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            var Node = Document.DocumentNode.ChildNodes.First(d => d.HasNameAttributeValue("section", "class", "readpage_top"));
            Node = Node.ChildNodes.First(d => d.HasNameAttributeValue("span", "class", "right"));
            Node = Node.ChildNodes.First(d => d.Name == "select");
            var Nodes = Node.ChildNodes.Where(d => d.Name == "option");

            var Output = Nodes.Select((d, e) => new PageInfo()
            {
                ParentChapter = Chapter,
                PageNo = e + 1,
                PageUri = new Uri(RootUri, d.Attributes["value"].Value)
            });
            return Output;
        }

        public override Uri GetImageUri(string MangaPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            var Node = Document.DocumentNode.ChildNodes.Where(d => d.Name == "img").First(d => d.HasAttributeValue("id", "image"));

            var Output = new Uri(Node.Attributes["src"].Value);
            return Output;
        }
    }
}
