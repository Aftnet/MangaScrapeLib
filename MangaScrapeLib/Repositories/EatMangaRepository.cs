﻿using MangaScrapeLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MangaScrapeLib.Repositories
{
    public class EatMangaRepository : MangaRepositoryBase, IRepository
    {
        public EatMangaRepository() : base("Eat Manga", "http://eatmanga.com/", "Manga-Scan/") { }

        public Series[] GetDefaultSeries(string MangaIndexPageHtml)
        {
            var Document = Parser.Parse(MangaIndexPageHtml);

            var Node = Document.QuerySelector("#updates");
            var Nodes = Node.QuerySelectorAll("th a");

            var Output = Nodes.Select(d => new Series(this, new Uri(RootUri, d.Attributes["href"].Value))
            {
                Name = d.TextContent
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

            var Node = Document.QuerySelector("#updates");
            var Nodes = Node.QuerySelectorAll("tr a");
            var Output = Nodes.Select(d =>
            {
                var NewChapterInfo = new Chapter(Series, new Uri(RootUri, d.Attributes["href"].Value))
                {
                    Title = d.TextContent
                };
                return NewChapterInfo;
            });

            //Eatmanga has dummy entries for not yet released chapters, prune them.
            Output = Output.Where(d => d.FirstPageUri.ToString().Contains("http://eatmanga.com/upcoming/") == false).Reverse();

            return Output.ToArray();
        }

        public Page[] GetPages(Chapter Chapter, string MangaPageHtml)
        {
            var Document = Parser.Parse(MangaPageHtml);

            var Node = Document.QuerySelector("#pages");
            var Nodes = Node.QuerySelectorAll("option");

            var Output = Nodes.Select((d, e) => new Page(Chapter, new Uri(RootUri, d.Attributes["value"].Value))
            {
                PageNo = e + 1
            }).OrderBy(d => d.PageNo);

            return Output.ToArray();
        }

        public Uri GetImageUri(string MangaPageHtml)
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
