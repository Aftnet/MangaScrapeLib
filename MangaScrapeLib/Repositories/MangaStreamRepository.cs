using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var output = new Uri(imageNode.Attributes["src"].Value);
            return output;
        }

        internal override IPage[] GetPages(Chapter chapter, string mangaPageHtml)
        {
            var document = Parser.Parse(mangaPageHtml);
            var listNode = document.QuerySelectorAll("ul.dropdown-menu")[2];
            
            var linksNodes = listNode.QuerySelectorAll("a");

            /* 
             * Mangastream doesnt't display all the pages in the Dropdown
             * if the pages count exceed a certain amount.
             * Parsing the last item needs to be done to get the pages count accurately.
             */
            try
            {
                var lastLinkNode = linksNodes.Last();

                var output = new List<Page>();

                var uri = lastLinkNode.Attributes["href"].Value;
                var lastPage = 0;

                var couldParseLastPageNumber = int.TryParse(UriTools.GetLastUriSegment(uri), out lastPage);

                if (!couldParseLastPageNumber)
                {
                    return new Page[0];
                }

                var baseUri = UriTools.TruncateLastUriSegment(uri);

                for (var i = 1; i <= lastPage; i++)
                {
                    output.Add(new Page(chapter, new Uri(baseUri + i), i));
                }

                return output.ToArray();
            }
            catch (InvalidOperationException e)
            {
                return new Page[0];
            }
        }
    }
}
