using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaStreamRepository : RepositoryBase
    {
        private Uri MangaIndexUri { get; }

        public MangaStreamRepository(IWebClient webClient) : base(webClient, "Mangastream", "https://www.mangastream.cc/", "MangaStream.png", false)
        {
            MangaIndexUri = new Uri(RootUri, "all-manga/");
        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(MangaIndexUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var itemNodes = document.QuerySelectorAll("div.page-item-detail");
            var output = itemNodes.Select(d =>
            {
                var titleNode = d.QuerySelector("div.post-title h3 a");
                var updateNode = d.QuerySelector("div.chapter-item span.post-on");
                var series = new Series(this, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent)
                {
                    Updated = updateNode.TextContent.Trim()
                };

                return series;
            }).ToArray();

            return output;
        }

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(Series input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var infoNode = document.QuerySelector("div.post-content");
            var authorNode = infoNode.QuerySelector("div.author-content a");
            var tagsNode = infoNode.QuerySelectorAll("div.genres-content a");
            var descriptionNode = document.QuerySelector("div.summary__content blockquote");

            input.Author = authorNode.TextContent.Trim();
            input.Tags = string.Join(", ", tagsNode.Select(d => d.TextContent.Trim()));
            input.Description = descriptionNode.TextContent;

            var rows = document.QuerySelectorAll("li.wp-manga-chapter").Skip(1);
            var output = rows.Reverse().Select((d, e) =>
            {
                var linkNode = d.QuerySelector("a");
                var datenode = d.QuerySelector("span i");
                var chapter = new Chapter((Series)input, new Uri(RootUri, linkNode.Attributes["href"].Value), linkNode.TextContent.Trim(), e) { Updated = datenode.TextContent.Trim() };
                return chapter;
            }).ToArray();

            return output;
        }

        internal override async Task<byte[]> GetImageAsync(Page input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

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

        internal override async Task<IReadOnlyList<IPage>> GetPagesAsync(Chapter input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

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
