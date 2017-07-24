﻿using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MangaScrapeLib.Repositories
{
    public sealed class EatMangaRepository : Repository
    {
        private static readonly EatMangaRepository instance = new EatMangaRepository();
        public static EatMangaRepository Instance { get { return instance; } }

        private EatMangaRepository() : base("Eat Manga", "http://eatmanga.com/", "Manga-Scan/", new SeriesMetadataSupport(), "EatManga.png") { }

        internal override ISeries[] GetDefaultSeries(string MangaIndexPageHtml)
        {
            var document = Parser.Parse(MangaIndexPageHtml);

            var rows = document.QuerySelectorAll("#updates tr");
            var output = new List<Series>();
            foreach (var i in rows)
            {
                var titleNode = i.QuerySelector("th a");
                var dateNode = i.QuerySelector("td.time");
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


        internal override void GetSeriesInfo(Series Series, string SeriesPageHtml)
        {
            Series.Description = string.Empty;
        }

        internal override IChapter[] GetChapters(Series Series, string SeriesPageHtml)
        {
            var Document = Parser.Parse(SeriesPageHtml);

            var Node = Document.QuerySelector("#updates");
            var Nodes = Node.QuerySelectorAll("tr a");
            var Timenodes = Node.QuerySelectorAll("td.time");
            var Output = Nodes.Zip(Timenodes, (d, e) => new Chapter(Series, new Uri(RootUri, d.Attributes["href"].Value), d.TextContent) { Updated = e.TextContent.Trim() }).ToArray();

            //Eatmanga has dummy entries for not yet released chapters, prune them.
            Output = Output.Where(d => d.FirstPageUri.ToString().Contains("http://eatmanga.com/upcoming/") == false).Reverse().ToArray();
            return Output;
        }

        internal override IPage[] GetPages(Chapter Chapter, string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("#pages");
            var Nodes = Node.QuerySelectorAll("option");

            var Output = Nodes.Select((d, e) => new Page(Chapter, new Uri(RootUri, d.Attributes["value"].Value), e + 1)).OrderBy(d => d.PageNo);
            return Output.ToArray();
        }

        internal override Uri GetImageUri(string MangaPageHtml)
        {
            var IDs = new string[]
            {
                "#eatmanga_image_big",
                "#eatmanga_image",
            };

            var Document = Parser.Parse(MangaPageHtml);

            foreach (var i in IDs)
            {
                var Node = Document.QuerySelector(i);
                if (Node == null) continue;

                return new Uri(Node.Attributes["src"].Value);
            }

            return null;
        }
    }
}
