using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;

namespace MangaScrapeLib.Repository
{
    internal class ReadmRepository : RepositoryBase
    {
        private Uri MangaIndexUri { get; }

        internal ReadmRepository(IWebClient webClient) : base(webClient, "Readm", "https://www.readm.org/", "Readm.png", true)
        {
            MangaIndexUri = new Uri(RootUri, "manga-list");
        }

        public async override Task<IReadOnlyList<ISeries>> GetSeriesAsync(CancellationToken token)
        {
            var html = await WebClient.GetStringAsync(MangaIndexUri, RootUri, token);
            if (html == null)
            {
                return null;
            }

            var document = await Parser.ParseDocumentAsync(html, token);
            if (token.IsCancellationRequested)
            {
                return null;
            }

            var nodes = document.QuerySelectorAll("li.segment-poster-sm");

            var output = nodes.Select(d =>
            {
                var titleNode = d.QuerySelector("h2");
                var imageNode = d.QuerySelector("img");
                var linkNode = d.QuerySelector("a");
                return new Series(this, new Uri(RootUri, linkNode.Attributes["href"].Value), titleNode.TextContent.Trim())
                {
                    CoverImageUri = new Uri(RootUri, imageNode.Attributes["data-src"].Value)
                };
            });

            return output.ToArray();
        }

        public override Task<IReadOnlyList<ISeries>> SearchSeriesAsync(string query, CancellationToken token)
        {
            return base.SearchSeriesAsync(query, token);
        }

        internal override Task<IReadOnlyList<IChapter>> GetChaptersAsync(Series input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        internal override Task<byte[]> GetImageAsync(Page input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        internal override Task<byte[]> GetImageAsync(ISeries input, CancellationToken token)
        {
            return base.GetImageAsync(input, token);
        }

        internal override Task<IReadOnlyList<IPage>> GetPagesAsync(Chapter input, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
