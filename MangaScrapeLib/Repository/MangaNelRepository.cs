﻿using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal class MangaNelRepository : RepositoryBase
    {
        [JsonObject]
        private class SearchEntry
        {
            [JsonProperty("name")]
            public string DisplayName { get; set; }
            [JsonProperty("nameunsigned")]
            public string UriBase { get; set; }
            [JsonProperty("image")]
            public string CoverUri { get; set; }
            [JsonProperty("author")]
            public string Author { get; set; }
        }

        private const string SearchUriPattern = "http://manganel.com/home/getjsonsearchstory?searchword={0}&search_style=tentruyen";
        private const string ReadUriPattern = "http://manganel.com/manga/{0}";
        private static readonly string[] SupuriousTitleText = { "<span style=\"color: #FF530D;font-weight: bold;\">", "</span>" };

        public MangaNelRepository() : base("Manga nel", "http://manganel.com/", "MangaNel.png", true)
        {
        }

        public override async Task<ISeries[]> GetSeriesAsync()
        {
            var html = await WebClient.GetStringAsync(RootUri, RootUri);
            var document = Parser.Parse(html);

            var seriesNodes = document.QuerySelectorAll("div.itemupdate.first");
            var output = seriesNodes.Select(d =>
            {
                var coverNode = d.QuerySelector("a.cover img");
                var coverUri = new Uri(RootUri, coverNode.Attributes["src"].Value);

                var titleNode = d.QuerySelector("ul li h3 a");
                var uri = new Uri(RootUri, titleNode.Attributes["href"].Value);
                var title = titleNode.TextContent;

                var dateNode = d.QuerySelector("ul li:nth-child(2) i");
                return new Series(this, uri, title)
                {
                    CoverImageUri = coverUri,
                    Updated = dateNode.TextContent
                };
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var uriQuery = Uri.EscapeDataString(query);
            var searchUri = new Uri(string.Format(SearchUriPattern, uriQuery));

            var json = await WebClient.GetStringAsync(searchUri, RootUri);
            var searchResult = JsonConvert.DeserializeObject<SearchEntry[]>(json);

            var output = searchResult.Select(d =>
            {
                var uri = new Uri(string.Format(ReadUriPattern, d.UriBase));
                var title = d.DisplayName;
                foreach(var i in SupuriousTitleText)
                {
                    title = title.Replace(i, string.Empty);
                }

                return new Series(this, uri, title)
                {
                    Author = d.Author,
                    CoverImageUri = new Uri(RootUri, d.CoverUri)
                };
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            var inputAsSeries = (Series)input;

            var html = await WebClient.GetStringAsync(input.SeriesPageUri, RootUri);
            var document = Parser.Parse(html);

            var infoNode = document.QuerySelector("ul.manga-info-text");
            var authorNode = infoNode.QuerySelector("li:nth-child(2) a");
            inputAsSeries.Author = authorNode.TextContent;
            var updatedNode = infoNode.QuerySelector("li:nth-child(4)");
            inputAsSeries.Updated = updatedNode.TextContent.Replace("Last updated : ", string.Empty);
            var tagNodes = infoNode.QuerySelectorAll("li:nth-child(7) a");
            inputAsSeries.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent).ToArray());
            var descriptionNode = document.QuerySelector("div#noidungm");
            inputAsSeries.Description = descriptionNode.TextContent.Trim();

            var listNode = document.QuerySelector("div#chapter div.manga-info-chapter div.chapter-list");
            var chapterNodes = listNode.QuerySelectorAll("div.row");
            var output = chapterNodes.Select(d =>
            {
                var titleNode = d.QuerySelector("span:nth-child(1) a");
                var dateNode = d.QuerySelector("span:nth-child(3)");

                return new Chapter(inputAsSeries, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent)
                {
                    Updated = dateNode.TextContent
                };
            }).ToArray();

            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri);
            var document = Parser.Parse(html);

            var listNode = document.QuerySelector("div#vungdoc");
            var imageNodes = listNode.QuerySelectorAll("img");
            var output = imageNodes.Select((d, e) =>
            {
                return new Page((Chapter)input, input.FirstPageUri, e + 1)
                {
                    ImageUri = new Uri(RootUri, d.Attributes["src"].Value)
                };
            }).ToArray();

            return output;
        }

        internal override Task<byte[]> GetImageAsync(IPage input)
        {
            return WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri);
        }
    }
}
