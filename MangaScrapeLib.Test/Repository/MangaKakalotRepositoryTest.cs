using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class MangaKakalotRepositoryTest : MangaRepositoryTestBase
    {
        public MangaKakalotRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaKakalotRepository(client);
        }
    }
}
