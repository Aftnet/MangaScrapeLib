using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;

namespace MangaScrapeLib.Repository
{
    internal sealed class SenMangaRepository : RepositoryBase
    {
        private static readonly Uri MangaIndexPage = new Uri("https://raw.senmanga.com/directory/popular");
        private const string UriSearchPattern = "https://raw.senmanga.com/ajax/search?q={0}";

        private class SeriesJson
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("author")]
            public string Author { get; set; }

            [JsonProperty("categories")]
            public string Tags { get; set; }

            [JsonProperty("img")]
            public string CoverUri { get; set; }

            [JsonProperty("url")]
            public string Uri { get; set; }
        }

        public SenMangaRepository(IWebClient webClient) : base(webClient, "Sen Manga", "https://raw.senmanga.com/", "SenManga.png", true)
        {

        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(MangaIndexPage, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var listNode = document.QuerySelector("ul.post-list");
            var titleNodes = listNode.QuerySelectorAll("li");


            var output = titleNodes.Select(d =>
            {
                var node = d.QuerySelector("a");
                var title = node.Attributes["title"].Value;
                var link = new Uri(RootUri, node.Attributes["href"].Value);
                var coverNode = node.QuerySelector("img");
                var cover = new Uri(RootUri, coverNode.Attributes["src"].Value);

                ISeries series = new Series(this, link, title) { CoverImageUri = cover };
                return series;
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
        {
            var searchUri = new Uri(string.Format(UriSearchPattern, query));
            var json = await WebClient.GetStringAsync(searchUri, RootUri, token);
            if (json == null)
            {
                return null;
            }

            try
            {
                var series = JsonConvert.DeserializeObject<SeriesJson[]>(json);
                var output = series.Select(d => new Series(this, new Uri(RootUri, d.Uri), d.Title) { Author = d.Author, Tags = d.Tags, CoverImageUri = new Uri(RootUri, d.CoverUri) } as ISeries)
                    .OrderBy(d => d.Title).ToArray();

                return output;
            }
            catch
            {
                return new ISeries[0];
            }
        }

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(ISeries input, CancellationToken token)
        {
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

            var series = input as Series;

            var coverNode = document.QuerySelector("div.thumbnail img");
            series.CoverImageUri = new Uri(RootUri, coverNode.Attributes["src"].Value);
            var infoNode = document.QuerySelector("div.info ul.series-info");
            var nodes = infoNode.QuerySelectorAll("li");

            series.Author = nodes[4].QuerySelector("a").TextContent;
            series.Description = nodes[1].QuerySelector("span").TextContent;
            series.Tags = string.Join(",", nodes[2].QuerySelectorAll("a").Select(d => d.TextContent));

            var chaptersNode = document.QuerySelector("div#content div.list div.group");
            nodes = chaptersNode.QuerySelectorAll("div.element");

            var output = nodes.Reverse().Select((d, e) =>
            {
                var titleNode = d.QuerySelector("div.title a");
                var metaNode = d.QuerySelector("div.meta_r");

                var title = titleNode.Attributes["title"].Value;
                var link = new Uri(RootUri, titleNode.Attributes["href"].Value);
                var date = metaNode.TextContent;
                return new Chapter(series, link, title, e) { Updated = date };
            }).ToArray();

            series.Updated = output.Last().Updated;
            return output;
        }

        internal override async Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var page = input as Page;

            var node = document.QuerySelector("img#picture");
            page.ImageUri = new Uri(RootUri, node.Attributes["src"].Value);

            var output = await WebClient.GetByteArrayAsync(page.ImageUri, input.PageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return output;
        }

        internal override async Task<IReadOnlyList<IPage>> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var chapter = input as Chapter;

            var uriRoot = input.FirstPageUri.ToString();
            uriRoot = uriRoot.Substring(0, uriRoot.LastIndexOf("/"));

            var node = document.QuerySelectorAll("select[name=page] option").Last();
            var lastPageNo = int.Parse(node.Attributes["value"].Value);
            var output = Enumerable.Range(1, lastPageNo).Select(d =>
            {
                var uri = new Uri(RootUri, $"{uriRoot}/{d}");
                return new Page(chapter, uri, d);
            }).ToArray();

            return output;
        }
    }
}
