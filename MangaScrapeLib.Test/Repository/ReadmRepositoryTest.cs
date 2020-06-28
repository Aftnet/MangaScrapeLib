using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class ReadmRepositoryTest : MangaRepositoryTestBase
    {
        public ReadmRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new ReadmRepository(client);
        }
    }
}
