using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal abstract class RepositoryBase : IRepository
    {
        internal abstract Task<IChapter[]> GetChaptersAsync(ISeries input);
        internal abstract Task<IPage[]> GetPagesAsync(IChapter input);
        internal abstract Task<byte[]> GetImageAsync(IPage input);

        private ISeries[] AvailableSeries { get; set; }

        protected static readonly HtmlParser Parser = new HtmlParser();

        private readonly string IconFileName;
        private readonly Lazy<byte[]> icon;
        public byte[] Icon { get { return icon.Value; } }

        public string Name { get; private set; }
        public Uri RootUri { get; private set; }
        public SeriesMetadataSupport SeriesMetadata { get; private set; }

        protected RepositoryBase(string name, string uriString, SeriesMetadataSupport seriesMetadata, string iconFileName)
        {
            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);
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
            if (AvailableSeries == null)
            {
                AvailableSeries = await GetSeriesAsync();
            }

            var lowerQuery = query.ToLowerInvariant();
            return AvailableSeries.Where(d => d.Title.Contains(lowerQuery)).OrderBy(d => d.Title).ToArray();
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
