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

        internal override Task<byte[]> GetImageAsync(Page input, CancellationToken token)
        {
            return WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri, token);
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

            var listNode = document.QuerySelector("div.reading-content");
            var imageNodes = listNode.QuerySelectorAll("img");
            var output = imageNodes.Select((d, e) => new Page(input, input.FirstPageUri, e + 1) { ImageUri = new Uri(RootUri, d.Attributes["src"].Value) }).ToArray();
            return output;          
        }
    }
}
