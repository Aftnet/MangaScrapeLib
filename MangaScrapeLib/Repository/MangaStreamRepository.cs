﻿using System;
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
    internal sealed class MangaStreamRepository : RepositoryBase
    {
        private class SearchResult
        {
            public class Item
            {
                public string Title { get; set; }
                public string Url { get; set; }
                public string Type { get; set; }
            }

            public bool Success { get; set; }
            public Item[] Data { get; set; }
        }

        private Uri MangaIndexUri { get; }
        private Uri SearchUri { get; }

        public MangaStreamRepository(IWebClient webClient) : base(webClient, "Mangastream", "https://www.mangastream.cc/", "MangaStream.png", false)
        {
            MangaIndexUri = new Uri(RootUri, "all-manga/");
            SearchUri = new Uri(RootUri, "wp-admin/admin-ajax.php");
        }

        public async override Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
        {
            var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("action", "wp-manga-search-manga"),
                new KeyValuePair<string, string>("title", query)
            });

            var response = await WebClient.PostAsync(content, SearchUri, RootUri, token);
            if (!response.IsSuccessStatusCode)
            {
                return new ISeries[0];
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SearchResult>(json);
            if (!result.Success)
            {
                return new ISeries[0];
            }

            return result.Data.Select(d => new Series(this, new Uri(d.Url, UriKind.Absolute), d.Title)).ToArray();
        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(MangaIndexUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var itemNodes = document.QuerySelectorAll("div.page-item-detail");
            var output = itemNodes.Select(d =>
            {
                var titleNode = d.QuerySelector<IHtmlAnchorElement>("div.post-title h3 a");
                var updateNode = d.QuerySelector("div.chapter-item span.post-on");
                var imageNode = d.QuerySelector<IHtmlImageElement>("div a img");
                var series = new Series(this, new Uri(titleNode.Href), titleNode.TextContent)
                {
                    CoverImageUri = new Uri(imageNode.Source),
                    Updated = updateNode.TextContent.Trim()
                };

                return series;
            }).ToArray();

            return output;
        }

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(Series input, CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(input.SeriesPageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var infoNode = document.QuerySelector("div.post-content");
            var authorNode = infoNode.QuerySelector("div.author-content a");
            var tagsNode = infoNode.QuerySelectorAll("div.genres-content a");
            var descriptionNode = document.QuerySelector("div.summary__content blockquote");
            var imageNode = document.QuerySelector<IHtmlImageElement>("div.summary_image a img");

            input.Author = authorNode.TextContent.Trim();
            input.Tags = string.Join(", ", tagsNode.Select(d => d.TextContent.Trim()));
            input.Description = descriptionNode.TextContent;
            input.CoverImageUri = new Uri(imageNode.Source);

            var rows = document.QuerySelectorAll("li.wp-manga-chapter").Skip(1);
            var output = rows.Reverse().Select((d, e) =>
            {
                var linkNode = d.QuerySelector<IHtmlAnchorElement>("a");
                var datenode = d.QuerySelector("span i");
                var chapter = new Chapter((Series)input, new Uri(linkNode.Href), linkNode.TextContent.Trim(), e) { Updated = datenode.TextContent.Trim() };
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
            var document = await BrowsingContext.OpenAsync(input.FirstPageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var listNode = document.QuerySelector("div.reading-content");
            var imageNodes = listNode.QuerySelectorAll<IHtmlImageElement>("img");
            var output = imageNodes.Select((d, e) => new Page(input, input.FirstPageUri, e + 1) { ImageUri = new Uri(d.Source) }).ToArray();
            return output;          
        }
    }
}
