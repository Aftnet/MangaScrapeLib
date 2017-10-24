using MangaScrapeLib.Repositories;

namespace MangaScrapeLib.Test.Repositories
{
    /// <summary>
    ///This is a test class for EatMangaRepositoryTest and is intended
    ///to contain all EatMangaRepositoryTest Unit Tests
    ///</summary>
    public class MangaEdenRepositoryTest : MangaRepositoryTestBase
    {
        protected override Repository GetRepository()
        {
            return MangaEdenRepository.Instance;
        }
    }
}
