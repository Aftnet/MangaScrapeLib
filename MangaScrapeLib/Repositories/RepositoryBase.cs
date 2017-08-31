using AngleSharp.Parser.Html;
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

        protected static readonly HtmlParser Parser = new HtmlParser();

        private readonly string IconFileName;
        private readonly Lazy<byte[]> icon;
        public byte[] Icon { get { return icon.Value; } }

        public string Name { get; private set; }
        public Uri RootUri { get; private set; }
        public Uri MangaIndexPage { get; private set; }
        public SeriesMetadataSupport SeriesMetadata { get; private set; }

        protected RepositoryBase(string name, string uriString, string mangaIndexPageStr, SeriesMetadataSupport seriesMetadata, string iconFileName)
        {
            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);
            MangaIndexPage = new Uri(RootUri, mangaIndexPageStr);
            SeriesMetadata = seriesMetadata;
            IconFileName = iconFileName;

            icon = new Lazy<byte[]>(LoadIcon);
        }

        public virtual Task<ISeries[]> GetSeriesAsync()
        {
            return Task.FromResult(new ISeries[0]);
        }

        public virtual async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var lowerQuery = query.ToLowerInvariant();
            var series = await GetSeriesAsync();
            var output = series.Where(d => d.Title.ToLowerInvariant().Contains(lowerQuery)).ToArray();
            return output;
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
