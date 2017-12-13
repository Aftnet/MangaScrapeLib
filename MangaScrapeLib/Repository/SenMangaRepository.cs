using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class SenMangaRepository : RepositoryBase
    {
        public SenMangaRepository() : base("Sen Manga", "https://raw.senmanga.com/", "SenManga.png", true)
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

        public override Task<ISeries[]> SearchSeriesAsync(string query)
        {
            return base.SearchSeriesAsync(query);
        }

        internal override Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            throw new NotImplementedException();
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
