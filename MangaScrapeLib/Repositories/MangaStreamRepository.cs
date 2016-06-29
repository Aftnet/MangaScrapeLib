using MangaScrapeLib.Models;
using System;
using System.Linq;

namespace MangaScrapeLib.Repositories
{
    public class MangaStreamRepository : Repository
    {
        private static readonly MangaStreamRepository instance = new MangaStreamRepository();
        public static MangaStreamRepository Instance { get { return instance; } }

        private MangaStreamRepository() : base("Mangastream", "http://mangastream.com/", "manga/", "MangaStream.png")
        {
        }

        internal override Series[] GetDefaultSeries(string mangaIndexPageHtml)
        {
            var document = Parser.Parse(mangaIndexPageHtml);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var linkNodes = tableNode.QuerySelectorAll("strong a");
            var output = linkNodes.Select(d => new Series(this, new Uri(d.Attributes["href"].Value), d.TextContent)).ToArray();
            return output;
        }

        internal override void GetSeriesInfo(Series series, string seriesPageHtml)
        {
        }

        internal override Chapter[] GetChapters(Series series, string seriesPageHtml)
        {
            var document = Parser.Parse(seriesPageHtml);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var linkNodes = tableNode.QuerySelectorAll("a");
            var output = linkNodes.Select(d => new Chapter(series, new Uri(d.Attributes["href"].Value), d.TextContent)).Reverse().ToArray();
            return output;
        }

        internal override Uri GetImageUri(string mangaPageHtml)
        {
            var document = Parser.Parse(mangaPageHtml);
            var imageNode = document.QuerySelector("img#manga-page");
            var output = new Uri(imageNode.Attributes["src"].Value);
            return output;
        }

        internal override Page[] GetPages(Chapter chapter, string mangaPageHtml)
        {
            var document = Parser.Parse(mangaPageHtml);
            var listNode = document.QuerySelectorAll("ul.dropdown-menu")[2];
            var linksNodes = listNode.QuerySelectorAll("a");
            var output = linksNodes.Select((d, e) => new Page(chapter, new Uri(d.Attributes["href"].Value), e + 1)).ToArray();
            return output;
        }
    }
}
