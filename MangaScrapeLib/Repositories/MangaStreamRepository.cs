using MangaScrapeLib.Models;
using System;
using System.Linq;

namespace MangaScrapeLib.Repositories
{
    public class MangaStreamRepository : Repository
    {
        private static readonly MangaStreamRepository instance = new MangaStreamRepository();
        public static MangaStreamRepository Instance { get { return instance; } }

        private MangaStreamRepository() : base("Mangastream", "http://mangastream.com/", "manga/", new SeriesMetadataSupport(), "MangaStream.png")
        {
        }

        internal override ISeries[] GetDefaultSeries(string mangaIndexPageHtml)
        {
            var document = Parser.Parse(mangaIndexPageHtml);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var linkNodes = tableNode.QuerySelectorAll("strong a");
            var updateNodes = tableNode.QuerySelectorAll("a.chapter-link");
            var output = linkNodes.Zip(updateNodes, (d, e) => new Series(this, new Uri(d.Attributes["href"].Value), d.TextContent) { Updated = e.TextContent }).ToArray();
            return output;
        }

        internal override void GetSeriesInfo(Series series, string seriesPageHtml)
        {
        }

        internal override IChapter[] GetChapters(Series series, string seriesPageHtml)
        {
            var document = Parser.Parse(seriesPageHtml);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var rows = tableNode.QuerySelectorAll("tr").Skip(1);
            var output = rows.Select(d =>
            {
                var linkNode = d.QuerySelector("a");
                var datenode = d.QuerySelectorAll("td").Skip(1);
                var chapter = new Chapter(series, new Uri(linkNode.Attributes["href"].Value), linkNode.TextContent) { Updated = datenode.First().TextContent.Trim() };
                return chapter;
            }).Reverse().ToArray();

            return output;
        }

        internal override Uri GetImageUri(string mangaPageHtml)
        {
            var document = Parser.Parse(mangaPageHtml);
            var imageNode = document.QuerySelector("img#manga-page");
            var output = new Uri($"http:{imageNode.Attributes["src"].Value}");
            return output;
        }

        internal override IPage[] GetPages(Chapter chapter, string mangaPageHtml)
        {
            var document = Parser.Parse(mangaPageHtml);
            var listNode = document.QuerySelectorAll("ul.dropdown-menu")[2];
            var linksNodes = listNode.QuerySelectorAll("a");
            var output = linksNodes.Select((d, e) => new Page(chapter, new Uri(d.Attributes["href"].Value), e + 1)).ToArray();
            return output;
        }
    }
}
