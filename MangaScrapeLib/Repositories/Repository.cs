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
                return new Repository[] { EatMangaRepository.Instance, MangaEdenRepository.Instance, MangaHereRepository.Instance, MangaStreamRepository.Instance, MyMangaRepository.Instance };
            }
        }

        protected static readonly HttpClient Client;
        protected static readonly HtmlParser Parser = new HtmlParser();

        public string Name { get; private set; }
        public byte[] Icon { get { return icon.Value; } }
        public Uri RootUri { get; private set; }
        public Uri MangaIndexPage { get; private set; }
        public SeriesMetadataSupport SeriesMetadata { get; private set; }

        private readonly string IconFileName;
        private readonly Lazy<byte[]> icon;


        internal abstract ISeries[] GetDefaultSeries(string mangaIndexPageHtml);
        internal abstract void GetSeriesInfo(Series series, string seriesPageHtml);
        internal abstract IChapter[] GetChapters(Series series, string seriesPageHtml);
        internal abstract IPage[] GetPages(Chapter chapter, string mangaPageHtml);
        internal abstract Uri GetImageUri(string mangaPageHtml);

        private ISeries[] DefaultSeries = null;

        static Repository()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:33.0) Gecko/20100101 Firefox/33.0");
        }

        protected Repository(string name, string uriString, string mangaIndexPageStr, SeriesMetadataSupport seriesMetadata, string iconFileName)
        {
            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);
            MangaIndexPage = new Uri(RootUri, mangaIndexPageStr);
            SeriesMetadata = seriesMetadata;
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

            var html = await GetStringAsync(MangaIndexPage, RootUri);
            DefaultSeries = GetDefaultSeries(html);
            DefaultSeries.OrderBy(d => d.Title).ToArray();
            return DefaultSeries;
        }

        public virtual async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var lowercaseQuery = query.ToLowerInvariant();
            var series = await GetSeriesAsync();
            return series.Where(d => d.Title.ToLowerInvariant().Contains(lowercaseQuery)).ToArray();
        }

        internal static async Task<IChapter[]> GetChaptersAsync(Series input)
        {
            var html = await GetStringAsync(input.SeriesPageUri, input.ParentRepository.MangaIndexPage);
            var repository = input.ParentRepository as Repository;
            repository.GetSeriesInfo(input, html);
            var output = repository.GetChapters(input, html);
            return output;
        }

        internal static async Task<IPage[]> GetPagesAsync(Chapter input)
        {
            var html = await GetStringAsync(input.FirstPageUri, input.ParentSeries.SeriesPageUri);
            var output = (input.ParentSeries.ParentRepository as Repository).GetPages(input, html);
            return output;
        }

        internal static async Task<byte[]> GetImageAsync(Page input)
        {
            var html = await GetStringAsync(input.PageUri, input.ParentChapter.FirstPageUri);
            input.ImageUri = (input.ParentChapter.ParentSeries.ParentRepository as Repository).GetImageUri(html);
            var output = await GetByteArrayAsync(input.ImageUri, input.PageUri);
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

        private static Task<string> GetStringAsync(Uri uri, Uri referrer)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.GetStringAsync(uri);
        }

        private static Task<byte[]> GetByteArrayAsync(Uri uri, Uri referrer)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.GetByteArrayAsync(uri);
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
