using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    internal abstract class RepositoryBase : IRepository
    {
        public abstract Task<ISeries[]> GetSeriesAsync();
        public abstract Task<ISeries[]> SearchSeriesAsync(string query);
        internal abstract Task<IChapter[]> GetChaptersAsync(ISeries input);
        internal abstract Task<IPage[]> GetPagesAsync(IChapter input);
        internal abstract Task<byte[]> GetImageAsync(IPage input);

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
