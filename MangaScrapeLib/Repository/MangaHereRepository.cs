using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaHereRepository : RepositoryBase
    {
        private static readonly Uri MangaIndexUri = new Uri("http://www.mangahere.co/mangalist/");

        public MangaHereRepository(WebClient webClient) : base(webClient, "Manga Here", "http://www.mangahere.co/", "MangaHere.png", false)
        {
        }

        public override async Task<ISeries[]> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(MangaIndexUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var nodes = document.QuerySelectorAll("a.manga_info");

            var output = nodes.Select(d => new Series(this, new Uri(RootUri, d.Attributes["href"].Value), WebUtility.HtmlDecode(d.Attributes["rel"].Value))).OrderBy(d => d.Title);
            return output.ToArray();
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var node = document.QuerySelector("div.manga_detail");
            node = node.QuerySelector("div.detail_list ul");
            var nodes = node.QuerySelectorAll("a.color_0077");

            var Output = nodes.Select(d =>
            {
                string Title = d.TextContent;
                Title = Regex.Replace(Title, @"^[\r\n\s\t]+", string.Empty);
                Title = Regex.Replace(Title, @"[\r\n\s\t]+$", string.Empty);
                var Chapter = new Chapter((Series)input, new Uri(RootUri, d.Attributes["href"].Value), Title);
                return Chapter;
            }).Reverse().ToArray();

            return Output.ToArray();
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var node = document.QuerySelector("section.readpage_top");
            node = node.QuerySelector("span.right select");
            var nodes = node.QuerySelectorAll("option");

            var Output = nodes.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Attributes["value"].Value), e + 1));
            return Output.ToArray();
        }

        internal override async Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri, token);
            if (html == null)
            {
                return null;
            }

            var document = Parser.Parse(html);

            var node = document.QuerySelector("img#image");
            var imageUri = new Uri(node.Attributes["src"].Value);

            ((Page)input).ImageUri = new Uri(RootUri, imageUri);
            var output = await WebClient.GetByteArrayAsync(input.ImageUri, input.PageUri, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            return output;
        }
    }
}
