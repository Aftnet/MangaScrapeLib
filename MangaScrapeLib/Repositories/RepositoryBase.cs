using AngleSharp.Parser.Html;
using MangaScrapeLib.Repositories.Components;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    internal abstract class RepositoryBase : IRepository
    {
        internal abstract Task<IChapter[]> GetChaptersAsync(ISeries input);
        internal abstract Task<IPage[]> GetPagesAsync(IChapter input);
        internal abstract Task<byte[]> GetImageAsync(IPage input);

        private readonly ISearcher SearchHandler;

        protected static readonly HtmlParser Parser = new HtmlParser();

        private readonly string IconFileName;
        private readonly Lazy<byte[]> icon;
        public byte[] Icon { get { return icon.Value; } }

        public string Name { get; private set; }
        public Uri RootUri { get; private set; }
        public Uri MangaIndexPage { get; private set; }
        public SeriesMetadataSupport SeriesMetadata { get; private set; }

        protected RepositoryBase(ISearcher searchHandler, string name, string uriString, string mangaIndexPageStr, SeriesMetadataSupport seriesMetadata, string iconFileName)
        {
            SearchHandler = searchHandler;

            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);
            MangaIndexPage = new Uri(RootUri, mangaIndexPageStr);
            SeriesMetadata = seriesMetadata;
            IconFileName = iconFileName;

            icon = new Lazy<byte[]>(LoadIcon);
        }

        public Task<ISeries[]> GetSeriesAsync()
        {
            return SearchHandler.GetSeriesAsync();
        }

        public Task<ISeries[]> SearchSeriesAsync(string query)
        {
            return SearchHandler.SearchSeriesAsync(query);
        }

        public override string ToString()
        {
            return Name;
        }

        private byte[] LoadIcon()
        {
            var iconPath = string.Format("MangaScrapeLib.Resources.{0}", IconFileName);
            var currentAssembly = typeof(RepositoryBase).GetTypeInfo().Assembly;
            using (var stream = currentAssembly.GetManifestResourceStream(iconPath))
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }
}
