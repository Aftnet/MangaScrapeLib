using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MyMangaRepository : RepositoryBase
    {
        [JsonObject]
        private class SearchEntry
        {
            [JsonProperty("manga")]
            public string MangaName { get; set; }
            [JsonProperty("permalink")]
            public string Permalink { get; set; }
        }

        private const string SeriesUpdatedValue = "Unknown";

        private static readonly Uri TopMangaPageUri = new Uri("http://www.heymanga.me/hot-manga/");

        public MyMangaRepository(IWebClient webClient) : base(webClient, "My Manga", "http://www.heymanga.me/", "MyManga.png", true)
        {
        }

        public override async Task<ISeries[]> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(TopMangaPageUri, RootUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var containerNode = document.QuerySelector("div.container");
            containerNode = containerNode.QuerySelector("div.row:nth-child(3)");
            var seriesDivs = containerNode.QuerySelectorAll("div.col-md-2");

            var output = seriesDivs.Select(d =>
            {
                var linkNode = d.QuerySelector("a");
                var titleNode = d.QuerySelector("div.manga-details");
                var series = new Series(this, new Uri(RootUri, linkNode.Attributes["href"].Value), titleNode.TextContent.Trim()) { Updated = SeriesUpdatedValue };
                return series;
            }).OrderBy(d => d.Title).ToArray();
            return output;
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query, CancellationToken token)
        {
            var uriQuery = Uri.EscapeDataString(query);
            var searchUri = new Uri(RootUri, string.Format("/inc/search.php?keyword={0}", uriQuery));

            var html = await WebClient.GetStringAsync(searchUri, RootUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var searchResult = JsonConvert.DeserializeObject<SearchEntry[]>(html);

            var uriFormatString = "manga/{0}";
            var output = searchResult.Select(d => new Series(this, new Uri(RootUri, string.Format(uriFormatString, d.Permalink)), d.MangaName) { Updated = SeriesUpdatedValue }).ToArray();
            return output;
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, RootUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var document = Parser.Parse(html);
            GetSeriesInfo((Series)input, html);

            var containerNode = document.QuerySelector("section.section-chapter div.container div.row:nth-child(2)");
            var linkNodes = containerNode.QuerySelectorAll("div.col-xs-9").Skip(1).ToArray();
            linkNodes = linkNodes.Select(d => d.QuerySelector("a")).ToArray();
            var dateNodes = containerNode.QuerySelectorAll("div.col-xs-3").Skip(1).ToArray();

            var output = linkNodes.Zip(dateNodes, (d, e) => new Chapter((Series)input, new Uri(RootUri, d.Attributes["href"].Value), d.TextContent.Trim()) { Updated = e.TextContent.Trim() })
                .Reverse().ToArray();
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

            var selectorNode = document.QuerySelector("select#page-dropdown");
            var optionNodes = selectorNode.QuerySelectorAll("option");

            var chaptersRootUri = input.FirstPageUri.ToString();
            chaptersRootUri = chaptersRootUri.Substring(0, chaptersRootUri.LastIndexOf("/"));
            var output = optionNodes.Select((d, e) => new Page((Chapter)input, new Uri(string.Format("{0}/{1}", chaptersRootUri, d.TextContent.Trim())), e + 1)).ToArray();
            return output;
        }

        internal override async Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var imageNode = document.QuerySelector("img.img-responsive");
            var uri = imageNode.Attributes["onerror"].Value;
            uri = uri.Substring(uri.IndexOf('\'') + 1);
            uri = uri.Substring(0, uri.IndexOf('\''));

            var inputAsPage = (Page)input;
            inputAsPage.ImageUri = new Uri(RootUri, uri);
            var output = await WebClient.GetByteArrayAsync(inputAsPage.ImageUri, input.PageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return output;
        }

        private void GetSeriesInfo(Series series, string seriesPageHtml)
        {
            var document = Parser.Parse(seriesPageHtml);

            var imageNode = document.QuerySelector("div.manga-cover center img");
            series.CoverImageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

            var detailsNode = document.QuerySelector("div.manga-data");
            var linkNodes = detailsNode.QuerySelectorAll("a").ToArray();
            var headerNodes = detailsNode.QuerySelectorAll("b").ToArray();

            var authorHeader = headerNodes.First(d => d.TextContent == "Author: ");
            series.Author = authorHeader.NextSibling.TextContent;

            var tagNodes = linkNodes.Where(d => d.HasAttribute("href")).Where(d => d.Attributes["href"].Value.Contains("/manga-directory/")).ToArray();
            series.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent));

            var descriptionHeader = headerNodes.First(d => d.TextContent == "Sypnosis: ");
            series.Description = descriptionHeader.NextSibling.NextSibling.TextContent.Trim();
        }
    }
}
