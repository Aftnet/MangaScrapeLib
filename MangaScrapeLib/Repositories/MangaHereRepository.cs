﻿using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    internal sealed class MangaHereRepository : RepositoryBase
    {
        private static readonly Uri MangaIndexUri = new Uri("http://www.mangahere.co/mangalist/");

        private MangaHereRepository() : base("Manga Here", "http://www.mangahere.co/", new SeriesMetadataSupport(), "MangaHere.png") { }

        public override async Task<ISeries[]> GetSeriesAsync()
        {
            var html = await WebClient.GetStringAsync(MangaIndexUri, RootUri);
            var document = Parser.Parse(html);

            var nodes = document.QuerySelectorAll("a.manga_info");

            var output = nodes.Select(d => new Series(this, new Uri(RootUri, d.Attributes["href"].Value), WebUtility.HtmlDecode(d.Attributes["rel"].Value))).OrderBy(d => d.Title);
            return output.ToArray();
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var series = await GetSeriesAsync();
            var lowerQuery = query.ToLowerInvariant();
            return series.Where(d => d.Title.Contains(lowerQuery)).OrderBy(d => d.Title).ToArray();
        }

        internal override async Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            var html = await WebClient.GetStringAsync(input.SeriesPageUri, MangaIndexUri);
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

        internal override async Task<byte[]> GetImageAsync(IPage input)
        {
            var html = await WebClient.GetStringAsync(input.PageUri, MangaIndexUri);
            var document = Parser.Parse(html);

            var node = document.QuerySelector("img#image");
            var imageUri = new Uri(node.Attributes["src"].Value);

            ((Page)input).ImageUri = new Uri(RootUri, imageUri);
            var output = await WebClient.GetByteArrayAsync(input.ImageUri, MangaIndexUri);
            return output;
        }

        internal override async Task<IPage[]> GetPagesAsync(IChapter input)
        {
            var html = await WebClient.GetStringAsync(input.FirstPageUri, MangaIndexUri);
            var document = Parser.Parse(html);

            var node = document.QuerySelector("section.readpage_top");
            node = node.QuerySelector("span.right select");
            var nodes = node.QuerySelectorAll("option");

            var Output = nodes.Select((d, e) => new Page((Chapter)input, new Uri(RootUri, d.Attributes["value"].Value), e + 1));
            return Output.ToArray();
        }
    }
}
