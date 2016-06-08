using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Repositories
{
    public class MangaHereRepository : MangaRepositoryBase, IRepository
    {
        public MangaHereRepository() : base("Manga Here", "http://www.mangahere.co/", "mangalist/") { }

        public IEnumerable<SeriesInfo> GetDefaultSeries(string MangaIndexPageHtml)
        {
            var Document = Parser.Parse(MangaIndexPageHtml);

            var Nodes = Document.QuerySelectorAll("a.manga_info");

            var Output = Nodes.Select(d => new SeriesInfo(this, new Uri(RootUri, d.Attributes["href"].Value))
            {
                Name = WebUtility.HtmlDecode(d.Attributes["rel"].Value)
            });
            Output = Output.OrderBy(d => d.Name);
            return Output;
        }

        public void GetSeriesInfo(SeriesInfo Series, string SeriesPageHtml)
        {
            Series.Description = Series.Tags = string.Empty;
        }

        public IEnumerable<ChapterInfo> GetChapters(SeriesInfo Series, string SeriesPageHtml)
        {
            var Document = Parser.Parse(SeriesPageHtml);

            var Node = Document.QuerySelector("div.manga_detail");
            Node = Node.QuerySelector("div.detail_list ul");
            var Nodes = Node.QuerySelectorAll("a.color_0077");

            var Output = Nodes.Select(d =>
            {
                string Title = d.TextContent;
                Title = Regex.Replace(Title, @"^[\r\n\s\t]+", string.Empty);
                Title = Regex.Replace(Title, @"[\r\n\s\t]+$", string.Empty);
                var Chapter = new ChapterInfo(Series, new Uri(RootUri, d.Attributes["href"].Value))
                {
                    Title = Title
                };

                return Chapter;
            });

            Output.Reverse();
            return Output;
        }

        public IEnumerable<PageInfo> GetPages(ChapterInfo Chapter, string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("section.readpage_top");
            Node = Node.QuerySelector("span.right select");
            var Nodes = Node.QuerySelectorAll("option");

            var Output = Nodes.Select((d, e) => new PageInfo(Chapter, new Uri(RootUri, d.Attributes["value"].Value))
            {
                PageNo = e + 1
            });
            return Output;
        }

        public Uri GetImageUri(string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("img#image");

            var Output = new Uri(Node.Attributes["src"].Value);
            return Output;
        }
    }
}
