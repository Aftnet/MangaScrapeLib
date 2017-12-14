using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class MangaNelRepositoryTest : MangaRepositoryTestBase
    {
        public MangaNelRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaNelRepository(client);
        }
    }
}
