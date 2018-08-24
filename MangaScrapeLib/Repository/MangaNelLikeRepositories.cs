using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal class MangaNelRepository : MangaNelLikeRepository
    {
        private const string RepoRootUriString = "https://manganelo.com/";

        public MangaNelRepository(IWebClient webClient) : base(webClient, "Manga NEL", RepoRootUriString, "MangaNel.png", $"{RepoRootUriString}home_json_search")
        {
        }
    }

    internal class MangaKakalotRepository : MangaNelLikeRepository
    {
        private const string RepoRootUriString = "http://mangakakalot.com/";

        public MangaKakalotRepository(IWebClient webClient) : base(webClient, "MangaKakalot", RepoRootUriString, "MangaKakalot.png", $"{RepoRootUriString}home_json_search")
        {
        }
    }

    internal class MangaBatRepository : MangaNelLikeRepository
    {
        private const string RepoRootUriString = "https://mangabat.com/";

        public MangaBatRepository(IWebClient webClient) : base(webClient, "MangaBat", RepoRootUriString, "MangaBat.png", $"{RepoRootUriString}getsearchstory")
        {
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var inputAsSeries = (Series)input;

            var html = await WebClient.GetStringAsync(input.SeriesPageUri, RootUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var imageNode = document.QuerySelector("div.truyen_info_left span.info_image img");
            inputAsSeries.CoverImageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

            var infoNode = document.QuerySelector("ul.truyen_info_right");
            var authorNode = infoNode.QuerySelector("li:nth-child(2) a");
            inputAsSeries.Author = authorNode.TextContent;
            var tagNodes = infoNode.QuerySelectorAll("li:nth-child(3) a");
            inputAsSeries.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent).ToArray());
            var updatedNode = infoNode.QuerySelector("li:nth-child(6) em em");
            inputAsSeries.Updated = updatedNode.TextContent;
            var descriptionNode = document.QuerySelector("div#noidungm");
            inputAsSeries.Description = descriptionNode.TextContent.Trim();
            inputAsSeries.Description = Regex.Replace(inputAsSeries.Description, @"[ \t\r\n]+", " ");

            var listNode = document.QuerySelector("div#list_chapter div.chapter-list");
            var chapterNodes = listNode.QuerySelectorAll("div.row");
            var output = chapterNodes.Select(d =>
            {
                var titleNode = d.QuerySelector("span:nth-child(1) a");
                var dateNode = d.QuerySelector("span:nth-child(2)");

                return new Chapter(inputAsSeries, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent)
                {
                    Updated = dateNode.TextContent
                };
            }).Reverse().ToArray();

            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var listNode = document.QuerySelector("div.vung_doc");
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
    }

    internal class MangaNelLikeRepository : RepositoryBase
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

        protected readonly string SearchUriPattern;
        protected readonly string ReadUriPattern;

        private static readonly string[] SupuriousTitleText = { "<span style=\"color: #FF530D;font-weight: bold;\">", "</span>" };

        protected MangaNelLikeRepository(IWebClient webClient, string name, string uriString, string iconFileName, string searchUriPattern) : base(webClient, name, uriString, iconFileName, true)
        {
            SearchUriPattern = searchUriPattern;
            ReadUriPattern = $"{RootUri.ToString()}manga/{{0}}";
        }

        public override async Task<ISeries[]> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(RootUri, RootUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

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
                return new Series(Repositories.DetermineOwnerRepository(uri) as RepositoryBase, uri, title)
                {
                    CoverImageUri = coverUri,
                    Updated = dateNode.TextContent
                };
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query, CancellationToken token)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrWhiteSpace(query))
            {
                return new ISeries[0];
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("searchword", query),
                new KeyValuePair<string, string>("search_style", "tentruyen")
            });

            var response = await WebClient.PostAsync(content, new Uri(SearchUriPattern), RootUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
            {
                return new ISeries[0];
            }

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

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var inputAsSeries = (Series)input;

            var html = await WebClient.GetStringAsync(input.SeriesPageUri, RootUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var imageNode = document.QuerySelector("div.manga-info-pic img");
            inputAsSeries.CoverImageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

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
            }).Reverse().ToArray();

            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

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

        internal override Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            return WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri, token);
        }
    }
}
