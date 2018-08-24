using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaDexRepository : RepositoryBase
    {
        private const string SearchUriBase = "https://mangadex.org/?page=search&title={0}";

        public MangaDexRepository(IWebClient webClient) : base(webClient, "MangaDex", "https://mangadex.org/", "MangaDex.png", true, true, false, true, true)
        {
        }

        public override async Task<ISeries[]> GetSeriesAsync(CancellationToken token)
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

        public override async Task<ISeries[]> SearchSeriesAsync(string query, CancellationToken token)
        {
            query = Uri.EscapeDataString(query);
            var builder = new Uri(string.Format(SearchUriBase, query));

            var html = await WebClient.GetStringAsync(RootUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            return null;
        }

        internal override Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        internal override Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        internal override Task<IPage[]> GetPagesAsync(IChapter input, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
