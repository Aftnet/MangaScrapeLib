using MangaScrapeLib.Models;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Repositories
{
    public sealed class MangaHereRepository : Repository
    {
        private static readonly MangaHereRepository instance = new MangaHereRepository();
        public static MangaHereRepository Instance { get { return instance; } }

        private MangaHereRepository() : base("Manga Here", "http://www.mangahere.co/", "mangalist/", "MangaHere.png") { }

        internal override ISeries[] GetDefaultSeries(string MangaIndexPageHtml)
        {
            var Document = Parser.Parse(MangaIndexPageHtml);

            var Nodes = Document.QuerySelectorAll("a.manga_info");

            var Output = Nodes.Select(d => new Series(this, new Uri(RootUri, d.Attributes["href"].Value), WebUtility.HtmlDecode(d.Attributes["rel"].Value))).OrderBy(d => d.Title);
            return Output.ToArray();
        }

        internal override void GetSeriesInfo(Series Series, string SeriesPageHtml)
        {
            Series.Description = string.Empty;
        }

        internal override IChapter[] GetChapters(Series Series, string SeriesPageHtml)
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
                var Chapter = new Chapter(Series, new Uri(RootUri, d.Attributes["href"].Value), Title);
                return Chapter;
            });

            Output.Reverse();
            return Output.ToArray();
        }

        internal override IPage[] GetPages(Chapter Chapter, string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("section.readpage_top");
            Node = Node.QuerySelector("span.right select");
            var Nodes = Node.QuerySelectorAll("option");

            var Output = Nodes.Select((d, e) => new Page(Chapter, new Uri(RootUri, d.Attributes["value"].Value), e + 1));
            return Output.ToArray();
        }

        internal override Uri GetImageUri(string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("img#image");

            var Output = new Uri(Node.Attributes["src"].Value);
            return Output;
        }
    }
}
