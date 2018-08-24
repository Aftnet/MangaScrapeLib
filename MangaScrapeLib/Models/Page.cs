using MangaScrapeLib.Tools;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    internal class Page : IPage
    {
        public Chapter ParentChapterInternal { get; private set; }
        public IChapter ParentChapter => ParentChapterInternal;

        public Uri PageUri { get; private set; }
        public int PageNo { get; private set; }

        public Uri ImageUri { get; internal set; }

        internal Page(Chapter parent, Uri pageUri, int pageNo)
        {
            ParentChapterInternal = parent;
            PageUri = pageUri;
            PageNo = pageNo;
        }

        public Task<byte[]> GetImageAsync()
        {
            using (var cts = new CancellationTokenSource())
            {
                return GetImageAsync(cts.Token);
            }
        }

        public virtual Task<byte[]> GetImageAsync(CancellationToken token)
        {
            return ParentChapterInternal.ParentSeriesInternal.ParentRepositoryInternal.GetImageAsync(this, token);
        }

        public virtual string SuggestPath(string rootDirectoryPath)
        {
            var pathStr = String.Format("{0}{1}{2}{3}", ImageUri.Scheme, "://", ImageUri.Authority, ImageUri.AbsolutePath);
            var extension = Path.GetExtension(pathStr);
            var output = Path.Combine(ParentChapter.SuggestPath(rootDirectoryPath), SuggestFileName().MakeValidPathName(), extension);
            return output;
        }

        public virtual string SuggestFileName()
        {
            const string numberFormatString = "000";
            var parentSeries = ParentChapter.ParentSeries;
            var output = string.Format("{0} P{1}", ParentChapter.Title, PageNo.ToString(numberFormatString));
            return output;
        }
    }
}
