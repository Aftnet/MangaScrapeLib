using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class EatMangaRepository : RepositoryBase
    {
        private static readonly Uri MangaIndexUri = new Uri("http://eatmanga.com/Manga-Scan/");

        private static readonly string[] ImageIDs = { "#eatmanga_image_big", "#eatmanga_image" };

        private ISeries[] AllSeries = null;

        public EatMangaRepository() : base("Eat Manga", "http://eatmanga.com/", new SeriesMetadataSupport(), "EatManga.png")
        {
        }

        public override async Task<ISeries[]> GetSeriesAsync()
        {
            if (AllSeries != null)
            {
                return AllSeries;
            }

            var mangaIndexPageHtml = await WebClient.GetStringAsync(MangaIndexUri, RootUri);
            var document = Parser.Parse(mangaIndexPageHtml);

            var rows = document.QuerySelectorAll("#updates li");
            var output = new List<Series>();
            foreach (var i in rows)
            {
                var titleNode = i.QuerySelector("a");
                var dateNode = i.QuerySelector("span.badge");
                if (titleNode == null || dateNode == null)
                {
                    continue;
                }

                var seriesUri = new Uri(RootUri, titleNode.Attributes["href"].Value);
                var series = new Series(this, seriesUri, titleNode.TextContent)
                {
                    Updated = dateNode.TextContent
                };
                output.Add(series);
            }

            return output.ToArray();
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri);
            var document = Parser.Parse(html);

            var node = document.QuerySelector("#updates");
            var nodes = node.QuerySelectorAll("li a");
            var timenodes = node.QuerySelectorAll("li span");
            var output = nodes.Zip(timenodes, (d, e) => new Chapter((Series)input, new Uri(RootUri, d.Attributes["href"].Value), d.TextContent) { Updated = e.TextContent.Trim() }).ToArray();

            //Eatmanga has dummy entries for not yet released chapters, prune them.
            output = output.Where(d => d.FirstPageUri.ToString().Contains("http://eatmanga.com/upcoming/") == false).Reverse().ToArray();
            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri);
            var document = Parser.Parse(html);

            var node = document.QuerySelector("#pages");
            var nodes = node.QuerySelectorAll("option");

            var output = nodes.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Attributes["value"].Value), e + 1)).OrderBy(d => d.PageNo);
            return output.ToArray();
        }

        internal override async Task<byte[]> GetImageAsync(IPage input)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri);
            var document = Parser.Parse(html);

            foreach (var i in ImageIDs)
            {
                var node = document.QuerySelector(i);
                var imgUri = node?.Attributes["src"].Value;

                if (imgUri != null)
                {
                    ((Page)input).ImageUri = new Uri(RootUri, imgUri);
                    var imgData = await WebClient.GetByteArrayAsync(new Uri(imgUri), input.PageUri);
                    return imgData;
                }
            }

            return null;
        }
    }
}
