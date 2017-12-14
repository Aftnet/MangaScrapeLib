using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public override async Task<ISeries[]> GetSeriesAsync()
        {
            var html = await WebClient.GetStringAsync(RootUri, RootUri);
            var document = Parser.Parse(html);

            var listNode = document.QuerySelector("div#content div.list");
            var titleNodes = listNode.QuerySelectorAll("div.group");
            var detailNodes = listNode.QuerySelectorAll("div.element");

            var output = titleNodes.Zip(detailNodes, (d, e) =>
            {
                var titleNode = d.QuerySelector("div.title a");
                var title = titleNode.Attributes["title"].Value;
                var link = new Uri(RootUri, titleNode.Attributes["href"].Value);
                var updatedNode = e.QuerySelector("div.meta_r");
                var updated = updatedNode.TextContent;

                ISeries series = new Series(this, link, title) { Updated = updated };
                return series;
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var searchUri = new Uri(string.Format(UriSearchPattern, query));
            var json = await WebClient.GetStringAsync(searchUri, RootUri);

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

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            var html = await WebClient.GetStringAsync(RootUri, RootUri);
            var document = Parser.Parse(html);

            var series = input as Series;

            var coverNode = document.QuerySelector("div.thumbnail img");
            series.CoverImageUri = new Uri(RootUri, coverNode.Attributes["src"].Value);
            var infoNode = document.QuerySelector("div.info ul.series-info");
            var nodes = infoNode.QuerySelectorAll("li");

            series.Author = nodes[4].QuerySelector("a").TextContent;
            series.Description = nodes[1].QuerySelector("span p").TextContent;
            series.Tags = string.Join(", ", nodes[2].QuerySelectorAll("a").Select(d => d.TextContent));
            series.Updated = "";

            var chaptersNode = document.QuerySelector("div#content div.list div.group");
            nodes = chaptersNode.QuerySelectorAll("div div a");

            var output = nodes.Select(d =>
            {
                var title = d.Attributes["title"].Value;
                var link = new Uri(RootUri, d.Attributes["href"].Value);
                return new Chapter(series, link, title);
            }).Reverse().ToArray();
            return output;
        }

        internal override Task<byte[]> GetImageAsync(IPage input)
        {
            throw new NotImplementedException();
        }

        internal override Task<IPage[]> GetPagesAsync(IChapter input)
        {
            throw new NotImplementedException();
        }
    }
}
