using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal sealed class MangaDexRepository : RepositoryBase
    {
        public MangaDexRepository(IWebClient webClient) : base(webClient, "MangaDex", "https://mangadex.org/", "MangaDex.png", true, true, false, true, true)
        {
        }

        internal override Task<IChapter[]> GetChaptersAsync(ISeries input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        internal override Task<byte[]> GetImageAsync(IPage input, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        internal override Task<IPage[]> GetPagesAsync(IChapter input, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
