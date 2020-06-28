using System;
using System.Collections.Generic;
using System.Linq;
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

        protected MangaEdenRepository(IWebClient webClient, string name, string mangaIndexUri, string searchLabelLanguageSuffix) : base(webClient, name, "http://www.mangaeden.com/", "MangaEden.png", false, true, true, true, true)
        {
            MangaIndexUri = new Uri(RootUri, mangaIndexUri);
            SearchLabelLanguageSuffix = searchLabelLanguageSuffix;
        }

        public override async Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(MangaIndexUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var table = document.QuerySelector("#mangaList");
            var rows = table.QuerySelectorAll("tr").Skip(1).ToArray();

            if (rows.Length == 1 && rows[0].TextContent.Contains("No results found"))
            {
                return new ISeries[0];
            }

            var output = rows.Select(d =>
            {
                var links = d.QuerySelectorAll<IHtmlAnchorElement>("a");
                var seriesLink = links.First();
                var updatesLink = links.Last();
                var updateText = updatesLink.TextContent.Replace('\n', ' ').Trim();
                var series = new Series(this, new Uri(seriesLink.Href), seriesLink.TextContent.Trim()) { Updated = updateText };
                return series;
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        public override async Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
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

        internal override async Task<IReadOnlyList<IChapter>> GetChaptersAsync(Series input, CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(input.SeriesPageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            GetSeriesInfo(input, document, token);

            var table = document.QuerySelector("table");
            var rows = table.QuerySelectorAll("tr").Skip(1);
            var output = rows.Reverse().Select((d, e) =>
            {
                var linknode = d.QuerySelector<IHtmlAnchorElement>("a.chapterLink");
                var titleNode = linknode.QuerySelector("b");
                var dateNode = d.QuerySelector("td.chapterDate");
                var chapter = new Chapter((Series)input, new Uri(linknode.Href), titleNode.TextContent.Trim(), e) { Updated = dateNode.TextContent.Trim() };
                return chapter;
            }).ToArray();

            input.Updated = output.Last().Updated;
            return output;
        }

        internal override async Task<IReadOnlyList<IPage>> GetPagesAsync(Chapter input, CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(input.FirstPageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var selectNode = document.QuerySelector<IHtmlSelectElement>("select#pageSelect");
            var output = selectNode.Options.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Value), e + 1)).ToArray();
            return output;
        }

        internal override async Task<byte[]> GetImageAsync(Page input, CancellationToken token)
        {
            var document = await BrowsingContext.OpenAsync(input.PageUri.ToString(), token);
            if (document == null || token.IsCancellationRequested)
            {
                return null;
            }

            var imageNode = document.QuerySelector<IHtmlImageElement>("img#mainImg");
            input.ImageUri = new Uri(imageNode.Source);

            var output = await WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return output;
        }

        private void GetSeriesInfo(Series series, IDocument document, CancellationToken token)
        {
            var imgNode = document.QuerySelector<IHtmlImageElement>("div.mangaImage2 img");
            if (imgNode != null)
            {
                series.CoverImageUri = new Uri(imgNode.Source);
            }

            var infoBoxNode = document.QuerySelectorAll("div.rightBox")[1];
            var headerNodes = infoBoxNode.QuerySelectorAll("h4");

            var linkNodes = infoBoxNode.QuerySelectorAll<IHtmlAnchorElement>("a").ToArray();
            var authorNode = linkNodes.First(d => d.Href.Contains("?author="));
            series.Author = authorNode.TextContent;

            var tagNodes = linkNodes.Where(d => d.Href.Contains("?categoriesInc=")).ToArray();
            series.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent));

            var descriptionNode = document.QuerySelector("h2#mangaDescription");
            series.Description = descriptionNode.TextContent.Trim();
        }
    }
}
