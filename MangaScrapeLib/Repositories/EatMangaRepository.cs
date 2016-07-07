using MangaScrapeLib.Models;
using System;
using System.Linq;

namespace MangaScrapeLib.Repositories
{
    public sealed class EatMangaRepository : Repository
    {
        private static readonly EatMangaRepository instance = new EatMangaRepository();
        public static EatMangaRepository Instance { get { return instance; } }

        private EatMangaRepository() : base("Eat Manga", "http://eatmanga.com/", "Manga-Scan/", "EatManga.png") { }

        internal override ISeries[] GetDefaultSeries(string MangaIndexPageHtml)
        {
            var Document = Parser.Parse(MangaIndexPageHtml);

            var Node = Document.QuerySelector("#updates");
            var Nodes = Node.QuerySelectorAll("th a");

            var Output = Nodes.Select(d => new Series(this, new Uri(RootUri, d.Attributes["href"].Value), d.TextContent)).OrderBy(d => d.Name);

            return Output.ToArray();
        }

        internal override void GetSeriesInfo(Series Series, string SeriesPageHtml)
        {
            Series.Description = Series.Tags = string.Empty;
        }

        internal override IChapter[] GetChapters(Series Series, string SeriesPageHtml)
        {
            var Document = Parser.Parse(SeriesPageHtml);

            var Node = Document.QuerySelector("#updates");
            var Nodes = Node.QuerySelectorAll("tr a");
            var Output = Nodes.Select(d => new Chapter(Series, new Uri(RootUri, d.Attributes["href"].Value), d.TextContent));

            //Eatmanga has dummy entries for not yet released chapters, prune them.
            Output = Output.Where(d => d.FirstPageUri.ToString().Contains("http://eatmanga.com/upcoming/") == false).Reverse();

            return Output.ToArray();
        }

        internal override IPage[] GetPages(Chapter Chapter, string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("#pages");
            var Nodes = Node.QuerySelectorAll("option");

            var Output = Nodes.Select((d, e) => new Page(Chapter, new Uri(RootUri, d.Attributes["value"].Value), e + 1)).OrderBy(d => d.PageNo);
            return Output.ToArray();
        }

        internal override Uri GetImageUri(string MangaPageHtml)
        {
            var IDs = new string[]
            {
                "#eatmanga_image_big",
                "#eatmanga_image",
            };

            var Document = Parser.Parse(MangaPageHtml);

            foreach (var i in IDs)
            {
                var Node = Document.QuerySelector(i);
                if (Node == null) continue;

                return new Uri(Node.Attributes["src"].Value);
            }

            return null;
        }
    }
}
