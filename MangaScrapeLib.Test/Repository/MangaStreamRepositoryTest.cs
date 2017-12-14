using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class MangaStreamRepositoryTest : MangaRepositoryTestBase
    {
        public MangaStreamRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaStreamRepository(client);
        }
    }
}
