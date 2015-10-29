using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.TestServices
{
    /// <summary>
    ///This is a test class for EatMangaRepositoryTest and is intended
    ///to contain all EatMangaRepositoryTest Unit Tests
    ///</summary>
    //[TestClass] Disabled since I can't access that server
    public class MangaFoxRepositoryTest : MangaRepositoryTestBase
    {
        internal override IMangaRepository GetRepository()
        {
            return new MangaFoxRepository();
        }
    }
}
