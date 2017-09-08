using AngleSharp.Parser.Html;
using MangaScrapeLib.Models;
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


        private readonly bool supportsCover;
        public bool SupportsCover => supportsCover;

        private readonly bool supportsAuthor;
        public bool SupportsAuthor => supportsAuthor;

        private readonly bool supportsLastUpdateTime;
        public bool SupportsLastUpdateTime => supportsLastUpdateTime;

        private readonly bool supportsTags;
        public bool SupportsTags => supportsTags;

        private readonly bool supportsDescription;
        public bool SupportsDescription => supportsDescription;

        protected RepositoryBase(string name, string uriString, string iconFileName, bool supportsAllMetadata) :
            this(name, uriString, iconFileName, supportsAllMetadata, supportsAllMetadata, supportsAllMetadata, supportsAllMetadata, supportsAllMetadata)
        {

        }

        protected RepositoryBase(string name, string uriString, string iconFileName, bool supportsCover, bool supportsAuthor, bool supportsLastUpdateTime, bool supportsTags, bool supportsDescription)
        {
            Name = name;
            RootUri = new Uri(uriString, UriKind.Absolute);

            IconFileName = iconFileName;

            icon = new Lazy<byte[]>(LoadIcon);

            this.supportsCover = supportsCover;
            this.supportsAuthor = supportsAuthor;
            this.supportsLastUpdateTime = supportsLastUpdateTime;
            this.supportsTags = supportsTags;
            this.supportsDescription = supportsDescription;
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

        public ISeries GetSeriesFromData(Uri seriesPageUri, string name)
        {
            if (seriesPageUri == null)
                return null;

            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                return null;

            if (RootUri.Host != seriesPageUri.Host)
                return null;

            return new Series(this, seriesPageUri, name);
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
