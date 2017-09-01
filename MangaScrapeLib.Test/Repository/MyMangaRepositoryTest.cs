namespace MangaScrapeLib.Test.Repository
{
    public class MyMangaRepositoryTest : MangaRepositoryTestBase
    {
        protected override IRepository Repository => Repositories.MyManga;
    }
}
