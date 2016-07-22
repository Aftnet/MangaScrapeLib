using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaScrapeLib.Models;
using Newtonsoft.Json;

namespace MangaScrapeLib.Repositories
{
    public class MyMangaRepository : NoFullListRepository
    {
        private const string SeriesUpdatedValue = "Unknown";

        private class SearchEntry
        {
            [JsonProperty("manga")]
            public string MangaName { get; set; }
            [JsonProperty("permalink")]
            public string Permalink { get; set; }
        }

        private static readonly MyMangaRepository instance = new MyMangaRepository();
        public static MyMangaRepository Instance { get { return instance; } }

        private MyMangaRepository() : base("My Manga", "http://www.mymanga.me/", "hot-manga/", new SeriesMetadataSupport(true), "MyManga.png")
        {
        }

        protected override Uri GetSearchUri(string query)
        {
            var uriQuery = Uri.EscapeDataString(query);
            return new Uri(RootUri, string.Format("/inc/search.php?keyword={0}", uriQuery));
        }

        protected override Series[] GetSeriesFromSearch(string searchPageHtml)
        {
            var searchResult = JsonConvert.DeserializeObject<SearchEntry[]>(searchPageHtml);
            var uriFormatString = "manga/{0}";
            var output = searchResult.Select(d => new Series(this, new Uri(RootUri, string.Format(uriFormatString, d.Permalink)), d.MangaName) { Updated = SeriesUpdatedValue }).ToArray();
            return output;
        }

        internal override IChapter[] GetChapters(Series series, string seriesPageHtml)
        {
            var document = Parser.Parse(seriesPageHtml);

            throw new NotImplementedException();
        }

        internal override ISeries[] GetDefaultSeries(string mangaIndexPageHtml)
        {
            var document = Parser.Parse(mangaIndexPageHtml);

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

        internal override Uri GetImageUri(string mangaPageHtml)
        {
            var document = Parser.Parse(mangaPageHtml);

            throw new NotImplementedException();
        }

        internal override IPage[] GetPages(Chapter chapter, string mangaPageHtml)
        {
            var document = Parser.Parse(mangaPageHtml);

            throw new NotImplementedException();
        }

        internal override void GetSeriesInfo(Series series, string seriesPageHtml)
        {
            var document = Parser.Parse(seriesPageHtml);

            var imageNode = document.QuerySelector("div.manga-cover center img");
            series.CoverImageUri = new Uri(RootUri, imageNode.Attributes["src"].Value);

            var detailsNode = document.QuerySelector("div.manga-data");
            var linkNodes = detailsNode.QuerySelectorAll("a").ToArray();
            var headerNodes = detailsNode.QuerySelectorAll("b").ToArray();

            var yearHeader = headerNodes.First(d => d.TextContent == "Year of Release: ");
            series.Release = yearHeader.NextSibling.TextContent;

            var authorHeader = headerNodes.First(d => d.TextContent == "Author: ");
            series.Author = authorHeader.NextSibling.TextContent;

            var tagNodes = linkNodes.Where(d => d.HasAttribute("href")).Where(d => d.Attributes["href"].Value.Contains("/manga-directory/")).ToArray();
            series.Tags = string.Join(", ", tagNodes.Select(d => d.TextContent));

            var descriptionHeader = headerNodes.First(d => d.TextContent == "Sypnosis: ");
            series.Description = descriptionHeader.NextSibling.NextSibling.TextContent.Trim();
        }
    }
}
