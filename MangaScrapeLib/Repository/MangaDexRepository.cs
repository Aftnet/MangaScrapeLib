using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaDexRepository : RepositoryBase
    {
        private const string ChaptersFilterLanguageCode = "gb";

        private IReadOnlyList<string> Genres { get; } = new string[] { "4-Koma",
            "Action", "Adventure", "Award Winning", "Comedy", "Cooking", "Doujinshi",
            "Drama", "Ecchi", "Fantasy", "Gender Bender", "Harem", "Historical", "Horror", "Josei",
            "Martial Arts", "Mecha", "Medical", "Music", "Mystery", "Oneshot", "Psychological",
            "Romance", "School Life", "Sci-Fi", "Seinen", "Shoujo", "Shoujo Ai", "Shounen",
            "Shounen Ai", "Slice of Life", "Smut", "Sports", "Supernatural", "Tragedy", "Webtoon",
            "Yaoi", "Yuri", "[no chapters]", "Game", "Isekai" };

        private class SeriesAPIResponse
        {
            public class SeriesInfo
            {
                [JsonProperty("cover_url")]
                public string CoverUri { get; set; }
                [JsonProperty("description")]
                public string Description { get; set; }
                [JsonProperty("title")]
                public string Title { get; set; }
                [JsonProperty("author")]
                public string Author { get; set; }
                [JsonProperty("genres")]
                public int[] Genres { get; set; }
            }

            public class ChapterInfo
            {
                [JsonIgnore]
                public int ID { get; set; }
                [JsonProperty("volume")]
                public string Volume { get; set; }
                [JsonProperty("chapter")]
                public string Chapter { get; set; }
                [JsonProperty("title")]
                public string Title { get; set; }
                [JsonProperty("lang_code")]
                public string LanguageCode { get; set; }
                [JsonProperty("timestamp")]
                public int TimeStamp { get; set; }
            }

            [JsonProperty("manga")]
            public SeriesInfo Series { get; set; }
            [JsonProperty("chapter")]
            public Dictionary<int, ChapterInfo> Chapters { get; set; }
        }

        [JsonObject]
        private class ChapterAPIResponse
        {
            [JsonProperty("id")]
            public string ChapterId { get; set; }
            [JsonProperty("server")]
            public string Server { get; set; }
            [JsonProperty("hash")]
            public string Hash { get; set; }
            [JsonProperty("page_array")]
            public string[] ImageFileNames { get; set; }
        }

        public MangaDexRepository(IWebClient webClient) : base(webClient, "MangaDex", "https://mangadex.org/", "MangaDex.png", true, true, false, true, true)
        {
        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(RootUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var table = document.QuerySelector("div#latest_update div.row");
            var items = table.QuerySelectorAll("div.col-md-6").ToArray();

            var output = items.Select(d =>
            {
                var imageNode = d.QuerySelector("div.sm_md_logo a img");
                var imgUri = new Uri(RootUri, imageNode.Attributes["src"].Value);
                var titleNode = d.QuerySelector("div.border-bottom.d-flex a");
                var series = new Series(this, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent.Trim()) { CoverImageUri = imgUri };
                return series;
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
        {
            query = Uri.EscapeDataString(query);
            var searchUriBase = "https://mangadex.org/?page=search&title={0}";
            var searchUri = new Uri(string.Format(searchUriBase, Uri.EscapeDataString(query)));

            var html = await WebClient.GetStringAsync(searchUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var noResultsAlert = document.QuerySelector("div#content div.alert-info");
            if (noResultsAlert != null)
            {
                return new ISeries[0];
            }

            var table = document.QuerySelector("div#content div.row.mt-1");
            var items = table.QuerySelectorAll("div.col-lg-6").ToArray();

            var output = items.Select(d =>
            {
                var imageNode = d.QuerySelector("div.rounded.large_logo a img");
                var imgUri = new Uri(RootUri, imageNode.Attributes["src"].Value);
                var titleNode = d.QuerySelector("div.text-truncate.d-flex a");
                var descriptionNode = d.Children[3];
                var series = new Series(this, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent.Trim()) { CoverImageUri = imgUri, Description = descriptionNode.TextContent };
                return series;
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var inputAsSeries = (Series)input;

            var regex = new Regex(@"title/([^/]+)");
            var seriesId = regex.Match(input.SeriesPageUri.ToString()).Groups[1].Value;
            var apiUri = $"api/?id={Uri.EscapeDataString(seriesId)}&type=manga";

            var json = await WebClient.GetStringAsync(new Uri(RootUri, apiUri), RootUri, token);
            if (json == null)
            {
                return null;
            }

            var seriesInfo = JsonConvert.DeserializeObject<SeriesAPIResponse>(json);
            inputAsSeries.Author = seriesInfo.Series.Author;
            inputAsSeries.Description = seriesInfo.Series.Description;
            inputAsSeries.CoverImageUri = new Uri(RootUri, seriesInfo.Series.CoverUri);

            var seriesGenres = seriesInfo.Series.Genres.Select(d => (d > 0 && d < Genres.Count) ? Genres[d] : null).Where(d => d != null).ToArray();
            inputAsSeries.Tags = seriesGenres.Any() ? string.Join(", ", seriesGenres) : "None";

            var chapters = seriesInfo.Chapters.Select(d =>
            {
                d.Value.ID = d.Key;
                return d.Value;
            }).Where(d => d.LanguageCode == ChaptersFilterLanguageCode).Reverse().ToArray();

            var output = chapters.Select((d, e) =>
                {
                    var chapterUri = $"chapter/{d.ID}";
                    var title = d.Title;
                    if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
                    {
                        title = $"Chapter {e + 1}";
                    }

                    var updated = DateTimeOffset.FromUnixTimeSeconds(d.TimeStamp);
                    var chapter = new Chapter(inputAsSeries, new Uri(RootUri, chapterUri), title, e) { Updated = updated.ToString("dd-MM-yyyy") };
                    return chapter;
                }).ToArray();

            return output;
        }

        internal override async Task<IReadOnlyList<IPage>> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var regex = new Regex(@"chapter/([^/]+)");
            var chapterId = regex.Match(input.FirstPageUri.ToString()).Groups[1].Value;
            var apiUri = $"api/?id={Uri.EscapeDataString(chapterId)}&type=chapter";

            var json = await WebClient.GetStringAsync(new Uri(RootUri, apiUri), RootUri, token);
            if (json == null)
            {
                return null;
            }

            var chapterInfo = JsonConvert.DeserializeObject<ChapterAPIResponse>(json);
            var imageUriFormat = @"{0}{1}/{2}";
            var output = chapterInfo.ImageFileNames.Select((d, e) => new Page((Chapter)input, input.FirstPageUri, e + 1)
            {
                ImageUri = new Uri(RootUri, string.Format(imageUriFormat, chapterInfo.Server, chapterInfo.Hash, d))
            }).ToArray();

            return output;
        }

        internal override Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            return WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri, token);
        }
    }
}
