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
                Name = WebUtility.HtmlDecode(d.InnerText).Trim(),
                SeriesPageUri = new Uri(RootUri, d.Attributes["href"].Value)
            });
            Output = Output.OrderBy(d => d.Name);
            return Output;
        }

        public override void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(SeriesPageHtml);

            var TopTextNode = Document.GetElementbyId("mangaproperties");
            var Node = TopTextNode.Descendants().First(d => d.InnerText == "Genre:");
            Node = Node.ParentNode;
            var Nodes = Node.Descendants().Where(d => d.Name == "a");
            Series.Tags = string.Join(", ", Nodes.Select(d => d.Attributes["href"].Value.Replace("/popular/", string.Empty)));

            Node = TopTextNode.ChildNodes.First(d => d.Name == "h1");
            Series.Description = WebUtility.HtmlDecode(Node.InnerText);
        }

        public override IEnumerable<ChapterInfo> GetChaptersList(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(SeriesPageHtml);

            var Node = Document.GetElementbyId("listing");
            var Nodes = Node.Descendants().Where(d => d.Name == "a");

            var Output = Nodes.Select(d => new ChapterInfo()
            {
                ParentSeries = Series,
                Title = WebUtility.HtmlDecode(d.InnerText),
                FirstPageUri = new Uri(RootUri, d.Attributes["href"].Value)
            });

            return Output;
        }

        public override IEnumerable<PageInfo> GetChapterPagesList(ChapterInfo Chapter, string MangaPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            var Node = Document.GetElementbyId("pageMenu");
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

            var Node = Document.GetElementbyId("img");

            var Output = new Uri(Node.Attributes["src"].Value);
            return Output;
        }
    }
}
