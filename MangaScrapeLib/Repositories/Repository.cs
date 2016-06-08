using AngleSharp.Parser.Html;
using MangaScrapeLib.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    public abstract class Repository
    {
        public static readonly Repository[] AllRepositories = new Repository[]
        {
            EatMangaRepository.Instance,
            MangaHereRepository.Instance
        };

        protected static readonly HttpClient Client = new HttpClient();
        protected static readonly HtmlParser Parser = new HtmlParser();

        public readonly string Name;
        public readonly Uri RootUri;
        public readonly Uri MangaIndexPage;

        private Series[] DefaultSeries = null;

        internal abstract Series[] GetDefaultSeries(string mangaIndexPageHtml);
        internal abstract void GetSeriesInfo(Series series, string seriesPageHtml);
        internal abstract Chapter[] GetChapters(Series series, string seriesPageHtml);
        internal abstract Page[] GetPages(Chapter chapter, string mangaPageHtml);
        internal abstract Uri GetImageUri(string mangaPageHtml);

        protected Repository(string name, string uriString, string mangaIndexPageStr)
        {
            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);
            MangaIndexPage = new Uri(RootUri, mangaIndexPageStr);
        }

        public override string ToString()
        {
            return Name;
        }

        public async Task<Series[]> GetSeriesAsync()
        {
            if (DefaultSeries != null) return DefaultSeries;

            var html = await Client.GetStringAsync(MangaIndexPage);
            DefaultSeries = GetDefaultSeries(html);
            DefaultSeries.OrderBy(d => d.Name).ToArray();
            return DefaultSeries;
        }

        public virtual async Task<Series[]> SearchSeriesAsync(string query)
        {
            var lowercaseQuery = query.ToLowerInvariant();
            var series = await GetSeriesAsync();
            return series.Where(d => d.Name.ToLowerInvariant().Contains(lowercaseQuery)).ToArray();
        }

        internal static async Task<Chapter[]> GetChaptersAsync(Series input)
        {
            var html = await Client.GetStringAsync(input.SeriesPageUri);
            input.ParentRepository.GetSeriesInfo(input, html);
            var output = input.ParentRepository.GetChapters(input, html);
            return output;
        }

        internal static async Task<Page[]> GetPagesAsync(Chapter input)
        {
            var html = await Client.GetStringAsync(input.FirstPageUri);
            var output = input.ParentSeries.ParentRepository.GetPages(input, html);
            return output;
        }

        internal static async Task<byte[]> GetImageAsync(Page input)
        {
            var html = await Client.GetStringAsync(input.PageUri);
            input.ImageUri = input.ParentChapter.ParentSeries.ParentRepository.GetImageUri(html);
            var output = await Client.GetByteArrayAsync(input.ImageUri);
            return output;
        }

        internal static string MakeValidPathName(string name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);
            name = Regex.Replace(name, invalidReStr, " ");
            name = Regex.Replace(name, @"\s{2,}", " ");
            name = Regex.Replace(name, @"^[\s]+", "");
            name = Regex.Replace(name, @"[\s]+$", "");
            name = Regex.Replace(name, @"[\s]+", " ");
            return name;
        }
    }
}
