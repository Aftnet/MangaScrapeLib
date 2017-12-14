using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaEdenRepository : RepositoryBase
    {
        private static readonly Uri MangaIndexUri = new Uri("http://www.mangaeden.com/en/en-directory/");

        public MangaEdenRepository(IWebClient webClient) : base(webClient, "Manga Eden", "http://www.mangaeden.com/", "MangaEden.png", true)
        {
        }

        public override Task<ISeries[]> GetSeriesAsync()
        {
            return SearchSeriesAsync(string.Empty);
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var uriQuery = Uri.EscapeDataString(query);
            var searchUri = new Uri(MangaIndexUri, string.Format("?title={0}", uriQuery));

            var html = await WebClient.GetStringAsync(searchUri, RootUri);
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

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            var inputAsSeries = (Series)input;
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri);
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

        internal override async Task<IPage[]> GetPagesAsync(IChapter input)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri);
            var document = Parser.Parse(html);

            var selectNode = document.QuerySelector("select#pageSelect");
            var options = selectNode.QuerySelectorAll("option");
            var output = options.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Attributes["value"].Value), e + 1)).ToArray();
            return output;
        }

        internal override async Task<byte[]> GetImageAsync(IPage input)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri);
            var document = Parser.Parse(html);

            var imageNode = document.QuerySelector("img#mainImg");
            var imageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

            ((Page)input).ImageUri = new Uri(RootUri, imageUri);
            var output = await WebClient.GetByteArrayAsync(imageUri, input.PageUri);
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
