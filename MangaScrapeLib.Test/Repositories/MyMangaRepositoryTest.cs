using MangaScrapeLib.Repositories;

namespace MangaScrapeLib.Test.Repositories
{
    public class MyMangaRepositoryTest : MangaRepositoryTestBase
    {
        protected override Repository GetRepository()
        {
            return MyMangaRepository.Instance;
        }
    }
}
