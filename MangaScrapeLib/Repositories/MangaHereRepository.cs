using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Repositories
{
    internal class MangaHereRepository : MangaRepositoryBase, IRepository
    {
        public MangaHereRepository() : base("Manga Here", "http://www.mangahere.co/", "mangalist/") { }

        public Series[] GetDefaultSeries(string MangaIndexPageHtml)
        {
            var Document = Parser.Parse(MangaIndexPageHtml);

            var Nodes = Document.QuerySelectorAll("a.manga_info");

            var Output = Nodes.Select(d => new Series(this, new Uri(RootUri, d.Attributes["href"].Value))
            {
                Name = WebUtility.HtmlDecode(d.Attributes["rel"].Value)
            }).OrderBy(d => d.Name);
            return Output.ToArray();
        }

        public void GetSeriesInfo(Series Series, string SeriesPageHtml)
        {
            Series.Description = Series.Tags = string.Empty;
        }

        public Chapter[] GetChapters(Series Series, string SeriesPageHtml)
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
                var Chapter = new Chapter(Series, new Uri(RootUri, d.Attributes["href"].Value))
                {
                    Title = Title
                };

                return Chapter;
            });

            Output.Reverse();
            return Output.ToArray();
        }

        public Page[] GetPages(Chapter Chapter, string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("section.readpage_top");
            Node = Node.QuerySelector("span.right select");
            var Nodes = Node.QuerySelectorAll("option");

            var Output = Nodes.Select((d, e) => new Page(Chapter, new Uri(RootUri, d.Attributes["value"].Value))
            {
                PageNo = e + 1
            });
            return Output.ToArray();
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
