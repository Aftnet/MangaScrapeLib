using HtmlAgilityPack;
using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MangaScrapeLib.Repositories
{
    public class EatMangaRepository : MangaRepository
    {
        public EatMangaRepository() : base("Eat Manga", "http://eatmanga.com/", "Manga-Scan/") { }

        public override IEnumerable<SeriesInfo> GetSeriesList(string MangaIndexPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaIndexPageHtml);

            var Node = Document.GetElementbyId("updates");
            var Nodes = Node.ChildNodes.Where(d => d.Name == "tr");
            Nodes = Nodes.Select(d => d.FirstChild).Where(d => d.Name == "th");
            Nodes = Nodes.Select(d => d.FirstChild);
            var Output = Nodes.Select(d => new SeriesInfo()
            {
                ParentRepository = this,
                Name = d.InnerText,
                SeriesPageUri = new Uri(RootUri, d.Attributes["href"].Value)
            });
            
            Output = Output.OrderBy(d => d.Name).ToList();
            return Output;
        }

        public override void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(SeriesPageHtml);

            var Node = Document.GetElementbyId("info_summary");
            Node = Node.ChildNodes.First(d => d.Name == "tbody");
            Node = Node.ChildNodes.Where(d => d.Name == "tr").ElementAt(1);
            Node = Node.ChildNodes.First(d => d.Name == "td");
            Series.Tags = Node.InnerText;

            Node = Document.GetElementbyId("info");
            Node = Node.ChildNodes.First(d => d.Name == "p");
            Series.Description = Node.InnerText;
        }

        public override IEnumerable<ChapterInfo> GetChaptersList(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(SeriesPageHtml);

            var Node = Document.GetElementbyId("updates");
            var Nodes = Node.ChildNodes.Where(d => d.Name == "tr");
            var Output = Nodes.Select(d =>
            {
                var AnchorNode = d.FirstChild.FirstChild;
                var NewChapterInfo = new ChapterInfo()
                {
                    ParentSeries = Series,
                    ChapterNo = -1,
                    Title = AnchorNode.InnerText,
                    FirstPageUri = new Uri(RootUri, AnchorNode.Attributes["href"].Value)
                };
                return NewChapterInfo;
            });

            //Eatmanga has dummy entries for not yet released chapters, prune them.
            Output = Output.Where(d => d.FirstPageUri.ToString().Contains("http://eatmanga.com/upcoming/") == false).Reverse();

            return Output;
        }

        public override IEnumerable<PageInfo> GetChapterPagesList(ChapterInfo Chapter, string MangaPageHtml)
        {
            var Document = new HtmlDocument();
            Document.LoadHtml(MangaPageHtml);

            var Node = Document.GetElementbyId("main_content");
            Node = Node.ChildNodes.Where(d => d.Name == "div").ElementAt(2);
            Node = Node.ChildNodes.Where(d => d.Name == "div").ElementAt(2);
            Node = Node.ChildNodes.Where(d => d.Name == "select").ElementAt(2);
            var Nodes = Node.ChildNodes.Where(d => d.Name == "option");

            var Output = Nodes.Select((d, e) => new PageInfo
            {
                ParentChapter = Chapter,
                PageNo = e + 1,
                PageUri = new Uri(RootUri, d.Attributes["value"].Value)
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
