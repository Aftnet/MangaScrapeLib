using AngleSharp.Parser.Html;
using MangaScrapeLib.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    public abstract class Repository : IRepository
    {
        public static Repository[] AllRepositories
        {
            get
            {
                return new Repository[] { EatMangaRepository.Instance, MangaHereRepository.Instance, MangaStreamRepository.Instance };
            }
        }

        protected static readonly HttpClient Client = new HttpClient();
        protected static readonly HtmlParser Parser = new HtmlParser();

        public string Name { get; private set; }
        public byte[] Icon { get { return icon.Value; } }
        public Uri RootUri { get; private set; }
        public Uri MangaIndexPage { get; private set; }

        private readonly string IconFileName;
        private readonly Lazy<byte[]> icon;


        internal abstract ISeries[] GetDefaultSeries(string mangaIndexPageHtml);
        internal abstract void GetSeriesInfo(Series series, string seriesPageHtml);
        internal abstract IChapter[] GetChapters(Series series, string seriesPageHtml);
        internal abstract IPage[] GetPages(Chapter chapter, string mangaPageHtml);
        internal abstract Uri GetImageUri(string mangaPageHtml);

        private ISeries[] DefaultSeries = null;

        protected Repository(string name, string uriString, string mangaIndexPageStr, string iconFileName)
        {
            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);
            MangaIndexPage = new Uri(RootUri, mangaIndexPageStr);
            IconFileName = iconFileName;

            icon = new Lazy<byte[]>(LoadIcon);
        }

        public override string ToString()
        {
            return Name;
        }

        public async Task<ISeries[]> GetSeriesAsync()
        {
            if (DefaultSeries != null) return DefaultSeries;

            var html = await Client.GetStringAsync(MangaIndexPage);
            DefaultSeries = GetDefaultSeries(html);
            DefaultSeries.OrderBy(d => d.Name).ToArray();
            return DefaultSeries;
        }

        public virtual async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var lowercaseQuery = query.ToLowerInvariant();
            var series = await GetSeriesAsync();
            return series.Where(d => d.Name.ToLowerInvariant().Contains(lowercaseQuery)).ToArray();
        }

        internal static async Task<IChapter[]> GetChaptersAsync(Series input)
        {
            var html = await Client.GetStringAsync(input.SeriesPageUri);
            input.ParentRepository.GetSeriesInfo(input, html);
            var output = input.ParentRepository.GetChapters(input, html);
            return output;
        }

        internal static async Task<IPage[]> GetPagesAsync(Chapter input)
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

        private byte[] LoadIcon()
        {
            var iconPath = string.Format("MangaScrapeLib.Resources.{0}", IconFileName);
            var currentAssembly = typeof(Repository).GetTypeInfo().Assembly;
            using (var stream = currentAssembly.GetManifestResourceStream(iconPath))
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }
}
