using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;

namespace MangaScrapeLib.Repository
{
    internal class ReadmRepository : RepositoryBase
    {
        private class SearchResult
        {
            public class SearchResultItem
            {
                public string Title { get; set; }
                public string Url { get; set; }
                public string Image { get; set; }
            }

            public SearchResultItem[] Manga { get; set; }
            public bool Success { get; set; }
        }

        private Uri MangaIndexUri { get; }
        private Uri MangaSearchUri { get; }

        internal ReadmRepository(IWebClient webClient) : base(webClient, "Readm", "https://www.readm.org/", "Readm.png", true)
        {
            MangaIndexUri = new Uri(RootUri, "manga-list");
            MangaSearchUri = new Uri(RootUri, "service/search");
        }

        public async override Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(MangaIndexUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var nodes = document.QuerySelectorAll("li.segment-poster-sm");

            var output = nodes.Select(d =>
            {
                var titleNode = d.QuerySelector("h2");
                var linkNode = d.QuerySelector<IHtmlAnchorElement>("a");
                return new Series(this, new Uri(linkNode.Href), titleNode.TextContent.Trim());
            });

            return output.ToArray();
        }

        public override async Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("phrase", query),
                new KeyValuePair<string, string>("dataType", "json")
            });

            var response = await WebClient.PostAsync(content, MangaSearchUri, MangaIndexUri, token);
            if (response == null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var searchResult = JsonConvert.DeserializeObject<SearchResult>(json);
            if (!searchResult.Success)
            {
                return new ISeries[0];
            }

            var output = searchResult.Manga.Select(d => new Series(this, new Uri(RootUri, d.Url), d.Title) { CoverImageUri = new Uri(RootUri, d.Image) }).ToArray();
            return output;
        }

        internal async override Task<IReadOnlyList<IChapter>> GetChaptersAsync(Series input, CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(input.SeriesPageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var imageNode = document.QuerySelector<IHtmlImageElement>("div#series-profile-wrapper img.series-profile-thumb");
            var authorNode = document.QuerySelector("span#first_episode small");
            var descriptionNode = document.QuerySelector("div.series-summary-wrapper");
            var tagsNodes = document.QuerySelectorAll<IHtmlAnchorElement>("a[data-navigo]");

            input.CoverImageUri = new Uri(RootUri, new Uri(imageNode.Source));
            input.Author = authorNode.TextContent;
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

            input.Updated = chapters.Last().Updated;

            return chapters;
        }

        internal async override Task<IReadOnlyList<IPage>> GetPagesAsync(Chapter input, CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(input.FirstPageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var imageNodes = document.QuerySelectorAll<IHtmlImageElement>("div.ch-images.ch-image-container img");
            var output = imageNodes.Select((d, e) => new Page(input, input.FirstPageUri, e + 1) { ImageUri = new Uri(d.Source) }).ToArray();
            return output;
        }

        internal override Task<byte[]> GetImageAsync(Page input, CancellationToken token)
        {
            return WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri, token);
        }
    }
}
