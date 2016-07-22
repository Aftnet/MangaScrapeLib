using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.Test.Repositories
{
    [TestClass]
    public class MyMangaRepositoryTest : MangaRepositoryTestBase
    {
        protected override Repository GetRepository()
        {
            return MyMangaRepository.Instance;
        }
    }
}
