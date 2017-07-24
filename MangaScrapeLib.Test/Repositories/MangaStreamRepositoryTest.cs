using MangaScrapeLib.Repositories;

namespace MangaScrapeLib.Test.Repositories
{
    public class MangaStreamRepositoryTest : MangaRepositoryTestBase
    {
        protected override Repository GetRepository()
        {
            return MangaStreamRepository.Instance;
        }
    }
}
