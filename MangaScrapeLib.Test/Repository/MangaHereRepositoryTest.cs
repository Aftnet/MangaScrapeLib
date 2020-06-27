using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class MangaHereRepositoryTest : MangaRepositoryTestBase
    {
        public MangaHereRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaHereRepository(client);
        }
    }
}
