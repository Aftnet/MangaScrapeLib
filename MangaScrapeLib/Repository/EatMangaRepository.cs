using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;

namespace MangaScrapeLib.Repository
{
    internal sealed class EatMangaRepository : RepositoryBase
    {
        private static Uri MangaIndexUri { get; } = new Uri("http://eatmanga.com/Manga-Scan/");

        private static string[] ImageIDs { get; } = { "#eatmanga_image_big", "#eatmanga_image" };

        private ISeries[] AllSeries { get; set; }

        public EatMangaRepository(IWebClient webClient) : base(webClient, "Eat Manga", "http://eatmanga.com/", "EatManga.png", false)
        {
        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            if (AllSeries != null)
            {
                return AllSeries;
            }

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

            var rows = document.QuerySelectorAll("#updates li");
            var output = new List<Series>();
            foreach (var i in rows)
            {
                var titleNode = i.QuerySelector<IHtmlAnchorElement>("a");
                var dateNode = i.QuerySelector("span.badge");
                if (titleNode == null || dateNode == null)
                {
                    continue;
                }

                var seriesUri = new Uri(RootUri, titleNode.Href);
                var series = new Series(this, seriesUri, titleNode.TextContent)
                {
                    Updated = dateNode.TextContent
                };
                output.Add(series);
            }

            return output.ToArray();
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

            var node = document.QuerySelector("#updates");
            var nodes = node.QuerySelectorAll<IHtmlAnchorElement>("li a");
            var timenodes = node.QuerySelectorAll("li span");
            var output = nodes.Zip(timenodes, (d, e) => new Chapter((Series)input, new Uri(RootUri, d.Href), d.TextContent, -1) { Updated = e.TextContent.Trim() }).ToArray();

            //Eatmanga has dummy entries for not yet released chapters, prune them.
            output = output.Where(d => d.FirstPageUri.ToString().Contains("http://eatmanga.com/upcoming/") == false).Reverse().ToArray();
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

            var node = document.QuerySelector("#pages");
            var nodes = node.QuerySelectorAll<IHtmlOptionElement>("option");

            var output = nodes.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Value), e + 1)).OrderBy(d => d.PageNumber);
            return output.ToArray();
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

            foreach (var i in ImageIDs)
            {
                var node = document.QuerySelector<IHtmlImageElement>(i);
                var imgUri = node?.Source;

                if (imgUri != null)
                {
                    ((Page)input).ImageUri = new Uri(RootUri, imgUri);
                    var imgData = await WebClient.GetByteArrayAsync(new Uri(imgUri), input.PageUri, token);
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    return imgData;
                }
            }

            return null;
        }
    }
}
