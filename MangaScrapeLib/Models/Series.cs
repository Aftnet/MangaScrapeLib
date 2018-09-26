using MangaScrapeLib.Repository;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    internal class Series : ISeries
    {
        public RepositoryBase ParentRepositoryInternal { get; private set; }
        public IRepository ParentRepository => ParentRepositoryInternal;

        public Uri SeriesPageUri { get; private set; }
        public string Title { get; private set; }
        public string Updated { get; internal set; }

        public Uri CoverImageUri { get; internal set; }
        public string Author { get; internal set; }
        public string Tags { get; internal set; }
        public string Description { get; internal set; }

        internal Series(RepositoryBase parent, Uri seriesPageUri, string title)
        {
            ParentRepositoryInternal = parent;
            SeriesPageUri = seriesPageUri;
            Title = title;
            Updated = string.Empty;
        }

        public Task<byte[]> GetCoverAsync()
        {
            using (var cts = new CancellationTokenSource())
            {
                return GetCoverAsync(cts.Token);
            }
        }

        public Task<byte[]> GetCoverAsync(CancellationToken token)
        {
            return ParentRepositoryInternal.GetImageAsync(this, token);
        }

        public Task<IReadOnlyList<IChapter>> GetChaptersAsync()
        {
            using (var cts = new CancellationTokenSource())
            {
                return GetChaptersAsync(cts.Token);
            }
        }

        public virtual Task<IReadOnlyList<IChapter>> GetChaptersAsync(CancellationToken token)
        {
            return ParentRepositoryInternal.GetChaptersAsync(this, token);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var output = Path.Combine(rootDirectoryPath, Title.MakeValidPathName());
            return output;
        }

        public IChapter GetSingleChapterFromData(Uri firstPageUri, string title, int readingOrder)
        {
            if (firstPageUri == null)
                return null;

            if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
                return null;

            if (firstPageUri.Host != SeriesPageUri.Host)
                return null;

            return new Chapter(this, firstPageUri, title, readingOrder);
        }
    }
}
