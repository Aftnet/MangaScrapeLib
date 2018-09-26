using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaDexRepository : RepositoryBase
    {
        [JsonObject]
        private class ChapterEntry
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

            var document = Parser.Parse(html);

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

            var document = Parser.Parse(html);

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
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var infoNode = document.QuerySelector("div.card-body div.row.edit");
            var imgNode = infoNode.QuerySelector("img.rounded");
            inputAsSeries.CoverImageUri = new Uri(RootUri, imgNode.Attributes["src"].Value);

            var infoNodes = infoNode.QuerySelectorAll("div.col-xl-9 div.row.border-top");
            var authorNode = infoNodes[0].QuerySelector("div a");
            inputAsSeries.Author = authorNode.TextContent.Trim();
            var tagNodes = infoNodes[2].QuerySelectorAll("a.genre");
            inputAsSeries.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent.Trim()));
            var descriptionNode = infoNodes[6].QuerySelector("div.col-lg-9");
            inputAsSeries.Description = descriptionNode.TextContent.Trim();

            var table = document.QuerySelector("div.chapter-container");
            var rows = table.Children.Skip(1);
            var output = rows.Reverse().Select((d, e) =>
            {
                d = d.QuerySelector("div div");
                var titleNode = d.QuerySelector("div.col.row.no-gutters.pr-1 a");
                var updateNode = d.QuerySelector("div.ml-1.order-lg-8");
                var chapter = new Chapter((Series)input, new Uri(RootUri, titleNode.Attributes["href"].Value), titleNode.TextContent.Trim(), e) { Updated = updateNode.TextContent.Trim() };
                return chapter;
            }).ToArray();

            inputAsSeries.Updated = output.Last().Updated;
            return output;
        }

        internal override async Task<IReadOnlyList<IPage>> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var regex = new Regex(@"chapter/([^/]+)");
            var chapterId = regex.Match(input.FirstPageUri.ToString()).Groups[1].Value;
            var apiUriBase = "https://mangadex.org/api/?id={0}&type=chapter";
            var apiUri = new Uri(string.Format(apiUriBase, Uri.EscapeDataString(chapterId)));

            var json = await WebClient.GetStringAsync(apiUri, RootUri, token);
            if (json == null)
            {
                return null;
            }

            var chapterInfo = JsonConvert.DeserializeObject<ChapterEntry>(json);
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
