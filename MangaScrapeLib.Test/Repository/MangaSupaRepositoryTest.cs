using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class MangaSupaRepositoryTest : MangaRepositoryTestBase
    {
        public MangaSupaRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaSupaRepository(client);
        }
    }
}
