using MangaScrapeLib.Repository;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MangaScrapeLib.Test")]

namespace MangaScrapeLib
{
    public static class Repositories
    {
        private static EatMangaRepository eatManga = new EatMangaRepository();
        public static IRepository EatManga => eatManga;

        private static MangaEdenRepository mangaEden = new MangaEdenRepository();
        public static IRepository MangaEden => mangaEden;

        private static MangaHereRepository mangaHere = new MangaHereRepository();
        public static IRepository MangaHere => mangaHere;

        private static MangaKakalotRepository mangaKakalot = new MangaKakalotRepository();
        public static IRepository MangaKakalot => mangaKakalot;

        private static MangaNelRepository mangaNel = new MangaNelRepository();
        public static IRepository MangaNel => mangaNel;

        private static MangaStreamRepository mangaStream = new MangaStreamRepository();
        public static IRepository MangaStream => mangaStream;

        private static MyMangaRepository myManga = new MyMangaRepository();
        public static IRepository MyManga => myManga;
    }
}
