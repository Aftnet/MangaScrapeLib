using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.Test
{
    /// <summary>
    ///This is a test class for EatMangaRepositoryTest and is intended
    ///to contain all EatMangaRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MangaFoxRepositoryTest : MangaRepositoryTestBase
    {
        internal override IMangaRepository GetRepository()
        {
            return new MangaFoxRepository();
        }
    }
}
