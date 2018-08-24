using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaEdenEnRepository : MangaEdenRepository
    {
        public MangaEdenEnRepository(IWebClient webClient) : base(webClient, "Manga Eden (EN)", "en/en-directory/", " - EN")
        {
        }
    }

    internal sealed class MangaEdenItRepository : MangaEdenRepository
    {
        public MangaEdenItRepository(IWebClient webClient) : base(webClient, "Manga Eden (IT)", "en/it-directory/", " - IT")
        {
        }
    }

    internal class MangaEdenRepository : RepositoryBase
    {
        private class JsonSeries
        {
            [JsonProperty("value")]
            public string Title { get; set; }

            [JsonProperty("url")]
            public string Uri { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }
        }

        private const string SearchUriPattern = "http://www.mangaeden.com/ajax/search-manga/?term={0}";

        private readonly Uri MangaIndexUri;
        private readonly string SearchLabelLanguageSuffix;

        protected MangaEdenRepository(IWebClient webClient, string name, string mangaIndexUri, string searchLabelLanguageSuffix) : base(webClient, name, "http://www.mangaeden.com/", "MangaEden.png", true)
        {
            MangaIndexUri = new Uri(RootUri, mangaIndexUri);
            SearchLabelLanguageSuffix = searchLabelLanguageSuffix;
        }

        public override async Task<ISeries[]> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(MangaIndexUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var table = document.QuerySelector("#mangaList");
            var rows = table.QuerySelectorAll("tr").Skip(1).ToArray();

            if (rows.Length == 1 && rows[0].TextContent.Contains("No results found"))
            {
                return new ISeries[0];
            }

            var output = rows.Select(d =>
            {
                var links = d.QuerySelectorAll("a");
                var seriesLink = links.First();
                var updatesLink = links.Last();
                var updateText = updatesLink.TextContent.Replace('\n', ' ').Trim();
                var series = new Series(this, new Uri(RootUri, seriesLink.Attributes["href"].Value), seriesLink.TextContent.Trim()) { Updated = updateText };
                return series;
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query, CancellationToken token)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrWhiteSpace(query))
            {
                return new ISeries[0];
            }

            var searchUri = new Uri(string.Format(SearchUriPattern, query));
            var json = await WebClient.GetStringAsync(searchUri, RootUri, token);
            if (json == null)
            {
                return null;
            }

            var result = JsonConvert.DeserializeObject<JsonSeries[]>(json);

            var output = result.Where(d => d.Label.EndsWith(SearchLabelLanguageSuffix))
                .Where(d => d.Title.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                .Select(d => new Series(this as RepositoryBase, new Uri(RootUri, d.Uri), d.Title) as ISeries)
                .OrderBy(d => d.Title).ToArray();
            return output;
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var inputAsSeries = (Series)input;
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri, token);
            if (html == null)
            {
                return null;
            }

            GetSeriesInfo(inputAsSeries, html);
            var document = Parser.Parse(html);

            var table = document.QuerySelector("table");
            var rows = table.QuerySelectorAll("tr").Skip(1);
            var output = rows.Select(d =>
            {
                var linknode = d.QuerySelector("a.chapterLink");
                var titleNode = linknode.QuerySelector("b");
                var dateNode = d.QuerySelector("td.chapterDate");
                var chapter = new Chapter((Series)input, new Uri(RootUri, linknode.Attributes["href"].Value), titleNode.TextContent.Trim()) { Updated = dateNode.TextContent.Trim() };
                return chapter;
            }).Reverse().ToArray();

            inputAsSeries.Updated = output.Last().Updated;
            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var selectNode = document.QuerySelector("select#pageSelect");
            var options = selectNode.QuerySelectorAll("option");
            var output = options.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Attributes["value"].Value), e + 1)).ToArray();
            return output;
        }

        internal override async Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var imageNode = document.QuerySelector("img#mainImg");
            var imageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

            ((Page)input).ImageUri = new Uri(RootUri, imageUri);
            var output = await WebClient.GetByteArrayAsync(imageUri, input.PageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return output;
        }

        private void GetSeriesInfo(Series series, string seriesPageHtml)
        {
            var document = Parser.Parse(seriesPageHtml);

            var imgNode = document.QuerySelector("div.mangaImage2 img");
            series.CoverImageUri = new Uri(RootUri, imgNode.Attributes["src"].Value);

            var infoBoxNode = document.QuerySelectorAll("div.rightBox")[1];
            var headerNodes = infoBoxNode.QuerySelectorAll("h4");

            var linkNodes = infoBoxNode.QuerySelectorAll("a").ToArray();
            var authorNode = linkNodes.First(d => d.Attributes["href"].Value.Contains("?author="));
            series.Author = authorNode.TextContent;

            var tagNodes = linkNodes.Where(d => d.Attributes["href"].Value.Contains("?categoriesInc=")).ToArray();
            series.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent));

            var descriptionNode = document.QuerySelector("h2#mangaDescription");
            series.Description = descriptionNode.TextContent.Trim();
        }
    }
}
