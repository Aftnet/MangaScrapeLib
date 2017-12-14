using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class SenMangaRepositoryTest : MangaRepositoryTestBase
    {
        public SenMangaRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new SenMangaRepository(client);
        }
    }
}
