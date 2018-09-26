using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    internal class Chapter : IChapter
    {
        internal Series ParentSeriesInternal { get; private set; }
        public ISeries ParentSeries => ParentSeriesInternal;

        public Uri FirstPageUri { get; private set; }
        public string Title { get; private set; }
        public int ReadingOrder { get; private set; }
        public string Updated { get; internal set; }

        internal Chapter(Series parent, Uri firstPageUri, string title, int readingOrder)
        {
            ParentSeriesInternal = parent;
            FirstPageUri = firstPageUri;
            Title = title;
            ReadingOrder = readingOrder;
            Updated = string.Empty;
        }

        public Task<IReadOnlyList<IPage>> GetPagesAsync()
        {
            using (var cts = new CancellationTokenSource())
            {
                return GetPagesAsync(cts.Token);
            }
        }

        public virtual Task<IReadOnlyList<IPage>> GetPagesAsync(CancellationToken token)
        {
            return ParentSeriesInternal.ParentRepositoryInternal.GetPagesAsync(this, token);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var parentSeriesPath = ParentSeries.SuggestPath(rootDirectoryPath);
            var output = Path.Combine(parentSeriesPath, Title.MakeValidPathName());
            return output;
        }
    }
}
