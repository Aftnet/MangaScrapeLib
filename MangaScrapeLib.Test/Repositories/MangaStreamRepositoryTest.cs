using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.Test.Repositories
{
    [TestClass]
    public class MangaStreamRepositoryTest : MangaRepositoryTestBase
    {
        protected override Repository GetRepository()
        {
            return new MangaStreamRepository();
        }
    }
}
