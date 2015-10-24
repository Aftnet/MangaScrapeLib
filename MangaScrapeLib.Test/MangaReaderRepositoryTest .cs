using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.Test
{
    /// <summary>
    ///This is a test class for MangaReaderRepository and is intended
    ///to contain all MangaReaderRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MangaReaderRepositoryTest : MangaRepositoryTestBase
    {
        internal override IMangaRepository GetRepository()
        {
            return new MangaReaderRepository();
        }
    }
}
