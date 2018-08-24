using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaStreamRepository : RepositoryBase
    {
        private static readonly Uri MangaIndexUri = new Uri("https://readms.net/manga");

        public MangaStreamRepository(IWebClient webClient) : base(webClient, "Mangastream", "https://readms.net/", "MangaStream.png", false)
        {
        }

        public override async Task<ISeries[]> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(MangaIndexUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var linkNodes = tableNode.QuerySelectorAll("strong a");
            var updateNodes = tableNode.QuerySelectorAll("a.chapter-link");
            var output = linkNodes.Zip(updateNodes, (d, e) =>
            {
                var uri = new Uri(RootUri, d.Attributes["href"].Value);
                var series = new Series(this, uri, d.TextContent) { Updated = e.TextContent };
                return series;
            }).ToArray();
            return output;
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);
            var tableNode = document.QuerySelector("table.table-striped") as AngleSharp.Dom.Html.IHtmlTableElement;
            var rows = tableNode.QuerySelectorAll("tr").Skip(1);
            var output = rows.Select(d =>
            {
                var linkNode = d.QuerySelector("a");
                var datenode = d.QuerySelectorAll("td").Skip(1);
                var chapter = new Chapter((Series)input, new Uri(RootUri, linkNode.Attributes["href"].Value), linkNode.TextContent) { Updated = datenode.First().TextContent.Trim() };
                return chapter;
            }).Reverse().ToArray();

            return output;
        }

        internal override async Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);
            var imageNode = document.QuerySelector("img#manga-page");

            var inputAsPage = (Page)input;
            inputAsPage.ImageUri = new Uri($"http:{imageNode.Attributes["src"].Value}");
            var output = await WebClient.GetByteArrayAsync(inputAsPage.ImageUri, input.PageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);
            var listNode = document.QuerySelector("div.btn-reader-page ul.dropdown-menu");

            var linksNodes = listNode.QuerySelectorAll("a");

            /* 
             * Mangastream doesn't display all the pages in the Dropdown
             * if the pages count exceeds a certain amount.
             * Parsing the last item needs to be done to get the pages count accurately.
             */
            try
            {
                var lastLinkNode = linksNodes.Last();

                var baseUri = new Uri(RootUri, lastLinkNode.Attributes["href"].Value);
                if (!int.TryParse(baseUri.Segments.Last(), out int lastPageNumber))
                {
                    return new Page[0];
                }

                var baseUriString = baseUri.ToString();
                var pageUris = Enumerable.Range(1, lastPageNumber).Select(d =>
                {
                    var pageUriString = baseUriString.Substring(0, baseUriString.LastIndexOf('/'));
                    pageUriString = $"{pageUriString}/{d}";
                    return new Uri(pageUriString);
                }).ToArray();

                var output = pageUris.Select((d, e) => new Page((Chapter)input, d, e + 1)).ToArray();
                return output;
            }
            catch (InvalidOperationException)
            {
                return new Page[0];
            }
        }
    }
}
