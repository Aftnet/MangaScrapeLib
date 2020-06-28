using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaHereRepository : RepositoryBase
    {
        private Uri MangaIndexUri { get; }

        public MangaHereRepository(IWebClient webClient) : base(webClient, "Manga Here", "http://www.mangahere.cc/", "MangaHere.png", false)
        {
            MangaIndexUri = new Uri(RootUri, "/directory");
        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(MangaIndexUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var nodes = document.QuerySelector("ul.manga-list-1-list").QuerySelectorAll("li");

            var output = nodes.Select(d =>
            {
                var linkNode = d.QuerySelector<IHtmlAnchorElement>("a");
                var imgNode = linkNode.QuerySelector<IHtmlImageElement>("img");
                return new Series(this, new Uri(linkNode.Href), linkNode.Title) { CoverImageUri = new Uri(imgNode.Source) };
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(Series input, CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(input.SeriesPageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var headerNode = document.QuerySelector("div.detail-info");
            var coverNode = headerNode.QuerySelector<IHtmlImageElement>("img.detail-info-cover-img");
            var authorNode = headerNode.QuerySelector<IHtmlAnchorElement>("p.detail-info-right-say a");
            var descriptionNode = headerNode.QuerySelector("p.detail-info-right-content");
            var tagNodes = headerNode.QuerySelectorAll<IHtmlAnchorElement>("p.detail-info-right-tag-list a");
            var updateNode = document.QuerySelector("span.detail-main-list-title-right");

            input.CoverImageUri = new Uri(coverNode.Source);
            input.Author = authorNode.Title;
            input.Description = descriptionNode.TextContent;
            input.Tags = string.Join(", ", tagNodes.Select(d => d.Title));
            input.Updated = updateNode.TextContent.Replace("Last Updated: ", string.Empty).Trim();

            var chapterNodes = document.QuerySelectorAll<IHtmlAnchorElement>("ul.detail-main-list a");

            var Output = chapterNodes.Reverse().Select((d, e) =>
            {
                var chUpdate = d.QuerySelector("p.title2");
                return new Chapter(input, new Uri(d.Href), d.Title, e) { Updated = chUpdate.TextContent.Trim() };
            }).ToArray();

            return Output.ToArray();
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

            var node = document.QuerySelector("section.readpage_top");
            node = node.QuerySelector("span.right select");
            var nodes = node.QuerySelectorAll("option");

            var Output = nodes.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Attributes["value"].Value), e + 1));
            return Output.ToArray();
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

            var node = document.QuerySelector("img#image");
            var imageUri = new Uri(node.Attributes["src"].Value);

            ((Page)input).ImageUri = new Uri(RootUri, imageUri);
            var output = await WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return output;
        }
    }
}
