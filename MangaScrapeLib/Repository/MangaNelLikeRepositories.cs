using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;

namespace MangaScrapeLib.Repository
{
    internal class MangaNelRepository : MangaNelLikeRepository
    {
        private const string RepoRootUriString = "https://manganelo.com/";

        public MangaNelRepository(IWebClient webClient) : base(webClient, "Manga NEL", RepoRootUriString, "MangaNel.png", RepoRootUriString, $"{RepoRootUriString}getstorysearchjson")
        {
        }
    }

    internal class MangaKakalotRepository : MangaNelLikeRepository
    {
        private const string RepoRootUriString = "https://mangakakalot.com/";

        public MangaKakalotRepository(IWebClient webClient) : base(webClient, "MangaKakalot", RepoRootUriString, "MangaKakalot.png", $"{RepoRootUriString}page", $"{RepoRootUriString}home_json_search")
        {
        }
    }

    internal class MangaBatRepository : MangaNelLikeRepository
    {
        private const string RepoRootUriString = "https://mangabat.com/";

        protected override string CoverImgXpath => "a.tooltip img.cover";

        public MangaBatRepository(IWebClient webClient) : base(webClient, "MangaBat", RepoRootUriString, "MangaBat.png", $"{RepoRootUriString}web", $"{RepoRootUriString}getsearchstory")
        {
        }

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var inputAsSeries = (Series)input;

            var html = await WebClient.GetStringAsync(input.SeriesPageUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var imageNode = document.QuerySelector("div.truyen_info_left img.info_image_manga");
            inputAsSeries.CoverImageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

            var infoNode = document.QuerySelector("ul.truyen_info_right");
            var authorNode = infoNode.QuerySelector("li:nth-child(2) a");
            inputAsSeries.Author = authorNode.TextContent;
            var tagNodes = infoNode.QuerySelectorAll("li:nth-child(3) a");
            if (tagNodes.Any())
            {
                inputAsSeries.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent).ToArray());
            }
            else
            {
                inputAsSeries.Tags = "NA";
            }

            var updatedNode = infoNode.QuerySelector("li:nth-child(6)");
            inputAsSeries.Updated = updatedNode.TextContent.Replace("Last updated :", string.Empty).Trim();
            var descriptionNode = document.QuerySelector("div#contentm");
            inputAsSeries.Description = descriptionNode.TextContent.Trim();
            inputAsSeries.Description = Regex.Replace(inputAsSeries.Description, @"[ \t\r\n]+", " ");

            var listNode = document.QuerySelector("div#list_chapter div.chapter-list");
            var chapterNodes = listNode.QuerySelectorAll("div.row");
            var output = chapterNodes.Reverse().Select((d, e) =>
            {
                var titleNode = d.QuerySelector("span:nth-child(1) a");
                var dateNode = d.QuerySelector("span:nth-child(2)");

                return new Chapter(inputAsSeries, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent, e)
                {
                    Updated = dateNode.TextContent
                };
            }).ToArray();

            return output;
        }

        internal override async Task<IReadOnlyList<IPage>> GetPagesAsync(IChapter input, CancellationToken token)
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

            var listNode = document.QuerySelector("div#vungdoc");
            if (listNode == null)
            {
                listNode = document.QuerySelector("div.vung_doc");
            }

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

        protected Uri FeaturedSeriesPageUri { get; }
        protected string SearchUriPattern { get; }
        protected string ReadUriPattern { get; }

        private static readonly string[] SupuriousTitleText = { "<span style=\"color: #FF530D;font-weight: bold;\">", "</span>" };

        protected virtual string CoverImgXpath => "a.item-img img";

        protected MangaNelLikeRepository(IWebClient webClient, string name, string uriString, string iconFileName, string featuredSeriesPageUri, string searchUriPattern) : base(webClient, name, uriString, iconFileName, true)
        {
            FeaturedSeriesPageUri = new Uri(featuredSeriesPageUri, UriKind.Absolute);
            SearchUriPattern = searchUriPattern;
            ReadUriPattern = $"{RootUri.ToString()}manga/{{0}}";
        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(FeaturedSeriesPageUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var seriesNodes = document.QuerySelectorAll("div.content-homepage-item");
            var output = seriesNodes.Select(d =>
            {
                var coverNode = d.QuerySelector(CoverImgXpath);
                var coverUri = new Uri(RootUri, coverNode.Attributes["src"].Value);

                var titleNode = d.QuerySelector("div h3.item-title a");
                var uri = new Uri(RootUri, titleNode.Attributes["href"].Value);
                var title = titleNode.TextContent;

                var dateNode = d.QuerySelector("div p.item-chapter i");
                var date = dateNode != null ? dateNode.TextContent : "Unknown";
                return new Series(Repositories.DetermineOwnerRepository(uri) as RepositoryBase, uri, title)
                {
                    CoverImageUri = coverUri,
                    Updated = date
                };
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrWhiteSpace(query))
            {
                return new ISeries[0];
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("searchword", query),
                //new KeyValuePair<string, string>("search_style", "tentruyen")
            });
            
            var response = await WebClient.PostAsync(content, new Uri(SearchUriPattern), FeaturedSeriesPageUri, token);
            if (response == null)
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

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var inputAsSeries = (Series)input;

            var html = await WebClient.GetStringAsync(input.SeriesPageUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var imageNode = document.QuerySelector("span.info-image img");
            inputAsSeries.CoverImageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

            var infoNode = document.QuerySelector("div.story-info-right");
            var authorNode = infoNode.QuerySelector("tr:nth-child(1) td.table-value a");
            inputAsSeries.Author = authorNode.TextContent;
            var updatedNode = infoNode.QuerySelector("div.story-info-right-extent span.stre-value");
            inputAsSeries.Updated = updatedNode.TextContent.Replace("Last updated : ", string.Empty);
            var tagNodes = infoNode.QuerySelectorAll("tr:nth-child(3) td.table-value a");
            inputAsSeries.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent).ToArray());
            var descriptionNode = document.QuerySelector("div.panel-story-info-description");
            inputAsSeries.Description = descriptionNode.TextContent.Trim();

            var listNode = document.QuerySelector("div.panel-story-chapter-list");
            var chapterNodes = listNode.QuerySelectorAll("li.a-h");
            var output = chapterNodes.Reverse().Select((d, e) =>
            {
                var titleNode = d.QuerySelector("a");
                var dateNode = d.QuerySelector("span.chapter-time");

                return new Chapter(inputAsSeries, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent, e)
                {
                    Updated = dateNode.TextContent
                };
            }).ToArray();

            return output;
        }

        internal override async Task<IReadOnlyList<IPage>> GetPagesAsync(IChapter input, CancellationToken token)
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

            var listNode = document.QuerySelector("div.container-chapter-reader");
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
