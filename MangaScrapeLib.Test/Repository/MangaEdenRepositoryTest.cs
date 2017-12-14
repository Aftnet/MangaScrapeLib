using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class MangaEdenRepositoryTest : MangaRepositoryTestBase
    {
        public MangaEdenRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaEdenRepository(client);
        }
    }
}
