using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal class MangaNelRepository : RepositoryBase
    {
        public MangaNelRepository() : base("Manga nel", "http://manganel.com/", "MangaNel.png", true)
        {
        }

        internal override Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            throw new NotImplementedException();
        }

        internal override Task<byte[]> GetImageAsync(IPage input)
        {
            throw new NotImplementedException();
        }

        internal override Task<IPage[]> GetPagesAsync(IChapter input)
        {
            throw new NotImplementedException();
        }
    }
}
