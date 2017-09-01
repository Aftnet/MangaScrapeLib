using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaStreamRepository : RepositoryBase
    {
        private static readonly Uri MangaIndexUri = new Uri("http://mangastream.com/manga/");

        public MangaStreamRepository() : base("Mangastream", "http://mangastream.com/", new SeriesMetadataSupport(), "MangaStream.png")
        {
        }

        public override async Task<ISeries[]> GetSeriesAsync()
        {
            var html = await WebClient.GetStringAsync(MangaIndexUri, RootUri);
            var document = Parser.Parse(html);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var linkNodes = tableNode.QuerySelectorAll("strong a");
            var updateNodes = tableNode.QuerySelectorAll("a.chapter-link");
            var output = linkNodes.Zip(updateNodes, (d, e) => new Series(this, new Uri(d.Attributes["href"].Value), d.TextContent) { Updated = e.TextContent }).ToArray();
            return output;
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri);
            var document = Parser.Parse(html);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var rows = tableNode.QuerySelectorAll("tr").Skip(1);
            var output = rows.Select(d =>
            {
                var linkNode = d.QuerySelector("a");
                var datenode = d.QuerySelectorAll("td").Skip(1);
                var chapter = new Chapter((Series)input, new Uri(linkNode.Attributes["href"].Value), linkNode.TextContent) { Updated = datenode.First().TextContent.Trim() };
                return chapter;
            }).Reverse().ToArray();

            return output;
        }

        internal override async Task<byte[]> GetImageAsync(IPage input)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri);
            var document = Parser.Parse(html);
            var imageNode = document.QuerySelector("img#manga-page");

            var inputAsPage = (Page)input;
            inputAsPage.ImageUri = new Uri($"http:{imageNode.Attributes["src"].Value}");
            var output = await WebClient.GetByteArrayAsync(inputAsPage.ImageUri, input.PageUri);
            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri);
            var document = Parser.Parse(html);
            var listNode = document.QuerySelectorAll("ul.dropdown-menu")[2];

            var linksNodes = listNode.QuerySelectorAll("a");

            /* 
             * Mangastream doesn't display all the pages in the Dropdown
             * if the pages count exceeds a certain amount.
             * Parsing the last item needs to be done to get the pages count accurately.
             */
            try
            {
                var lastLinkNode = linksNodes.Last();

                var output = new List<Page>();

                var uri = lastLinkNode.Attributes["href"].Value;
                var lastPage = 0;

                var couldParseLastPageNumber = int.TryParse(ExtensionMethods.GetLastUriSegment(uri), out lastPage);

                if (!couldParseLastPageNumber)
                {
                    return new Page[0];
                }

                var baseUri = ExtensionMethods.TruncateLastUriSegment(uri);

                for (var i = 1; i <= lastPage; i++)
                {
                    output.Add(new Page((Chapter)input, new Uri(baseUri + i), i));
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
