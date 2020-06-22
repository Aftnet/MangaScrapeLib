using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;

namespace MangaScrapeLib.Repository
{
    internal class ReadmRepository : RepositoryBase
    {
        private Uri MangaIndexUri { get; }

        internal ReadmRepository(IWebClient webClient) : base(webClient, "Readm", "https://www.readm.org/", "Readm.png", true)
        {
            MangaIndexUri = new Uri(RootUri, "manga-list");
        }

        public async override Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
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

            var nodes = document.QuerySelectorAll("li.segment-poster-sm");

            var output = nodes.Select(d =>
            {
                var titleNode = d.QuerySelector("h2");
                var imageNode = d.QuerySelector("img");
                var linkNode = d.QuerySelector("a");
                return new Series(this, new Uri(RootUri, linkNode.Attributes["href"].Value), titleNode.TextContent.Trim())
                {
                    CoverImageUri = new Uri(RootUri, imageNode.Attributes["data-src"].Value)
                };
            });

            return output.ToArray();
        }

        internal async override Task<IReadOnlyList<IChapter>> GetChaptersAsync(Series input, CancellationToken token)
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

            var imageNode = document.QuerySelector<IHtmlImageElement>("div#series-profile-wrapper img.series-profile-thumb");
            var descriptionNode = document.QuerySelector("div.series-summary-wrapper p");
            var tagsNodes = document.QuerySelectorAll<IHtmlAnchorElement>("a[data-navigo]");

            input.CoverImageUri = new Uri(RootUri, new Uri(RootUri, imageNode.Source));
            input.Description = descriptionNode.TextContent.Trim();
            input.Tags = string.Join(", ", tagsNodes.Select(d => d.TextContent.Trim()));

            var chapters = document.QuerySelectorAll("div.season-list-column tr").Reverse().Select((d, e) =>
            {
                var link = d.QuerySelector<IHtmlAnchorElement>("a");
                var date = d.QuerySelector("td.episode-date");
                return new Chapter(input, new Uri(RootUri, link.Href), link.TextContent, e)
                {
                    Updated = date.TextContent.Trim()
                };
            }).ToArray();
           
            return chapters;
        }

        internal async override Task<IReadOnlyList<IPage>> GetPagesAsync(Chapter input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        internal async override Task<byte[]> GetImageAsync(Page input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async override Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
