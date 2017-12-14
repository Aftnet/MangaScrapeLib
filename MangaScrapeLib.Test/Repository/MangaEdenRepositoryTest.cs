using MangaScrapeLib.Repository;
using MangaScrapeLib.Test.Tools;

namespace MangaScrapeLib.Test.Repository
{
    public class MangaEdenEnRepositoryTest : MangaRepositoryTestBase
    {
        public MangaEdenEnRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaEdenEnRepository(client);
        }
    }

    public class MangaEdenItRepositoryTest : MangaRepositoryTestBase
    {
        public MangaEdenItRepositoryTest(WebCache client) : base(client)
        {
        }

        protected override IRepository GenerateRepository(WebCache client)
        {
            return new MangaEdenItRepository(client);
        }
    }
}
