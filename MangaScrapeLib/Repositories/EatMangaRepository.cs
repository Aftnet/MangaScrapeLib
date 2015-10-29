using HtmlAgilityPack;
using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MangaScrapeLib.Repositories
{
    public class EatMangaRepository : MangaRepositoryBase
    {
        public EatMangaRepository() : base("Eat Manga", "http://eatmanga.com/", "Manga-Scan/") { }

        public override IEnumerable<SeriesInfo> GetSeries(string MangaIndexPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaIndexPageHtml);

            var Node = Document.GetElementbyId("updates");
            var Nodes = Node.ChildNodes.Where(d => d.Name == "tr");
            Nodes = Nodes.Select(d => d.FirstChild).Where(d => d.Name == "th");
            Nodes = Nodes.Select(d => d.FirstChild);
            var Output = Nodes.Select(d => new SeriesInfo(this, new Uri(RootUri, d.Attributes["href"].Value))
            {
                Name = d.InnerText
            });
            
            Output = Output.OrderBy(d => d.Name).ToList();
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

            var Node = Document.GetElementbyId("updates");
            var Nodes = Node.ChildNodes.Where(d => d.Name == "tr");
            var Output = Nodes.Select(d =>
            {
                var AnchorNode = d.FirstChild.FirstChild;
                var NewChapterInfo = new ChapterInfo(Series, new Uri(RootUri, AnchorNode.Attributes["href"].Value))
                {
                    ChapterNo = -1,
                    Title = AnchorNode.InnerText
                };
                return NewChapterInfo;
            });

            //Eatmanga has dummy entries for not yet released chapters, prune them.
            Output = Output.Where(d => d.FirstPageUri.ToString().Contains("http://eatmanga.com/upcoming/") == false).Reverse();

            return Output;
        }

        public override IEnumerable<PageInfo> GetPages(ChapterInfo Chapter, string MangaPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            var Node = Document.GetElementbyId("pages");
            var Nodes = Node.ChildNodes.Where(d => d.Name == "option");

            var Output = Nodes.Select((d, e) => new PageInfo(Chapter, new Uri(RootUri, d.Attributes["value"].Value))
            {
                PageNo = e + 1
            });
            Output = Output.OrderBy(d => d.PageNo);
            return Output;
        }

        public override Uri GetImageUri(string MangaPageHtml)
        {
            var IDs = new string[]
            {
                "eatmanga_image_big",
                "eatmanga_image",
            };

            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            foreach (var i in IDs)
            {
                var Node = Document.GetElementbyId(i);
                if (Node == null) continue;

                return new Uri(Node.Attributes["src"].Value);
            }

            return null;
        }
    }
}
