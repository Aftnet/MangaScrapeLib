using MangaScrapeLibPortable.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLibPortable.Test
{
    /// <summary>
    ///This is a test class for EatMangaRepositoryTest and is intended
    ///to contain all EatMangaRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MangaHereRepositoryTest : MangaRepositoryTest
    {
        internal override MangaRepository CreateMangaRepository()
        {
            return new MangaHereRepository();
        }
    }
}
