using MangaScrapeLib.Repository;
using System;

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
        //public static IRepository MangaHere => mangaHere;

        private static MangaKakalotRepository mangaKakalot = new MangaKakalotRepository();
        public static IRepository MangaKakalot => mangaKakalot;

        private static MangaNelRepository mangaNel = new MangaNelRepository();
        public static IRepository MangaNel => mangaNel;

        private static MangaStreamRepository mangaStream = new MangaStreamRepository();
        public static IRepository MangaStream => mangaStream;

        private static MangaSupaRepository mangaSupa = new MangaSupaRepository();
        public static IRepository MangaSupa => mangaSupa;

        private static MyMangaRepository myManga = new MyMangaRepository();
        //public static IRepository MyManga => myManga;

        private static SenMangaRepository senManga = new SenMangaRepository();
        public static IRepository SenManga => senManga;

        private static IRepository[] allRepositories = { EatManga, MangaEden, MangaKakalot, MangaNel, MangaStream, MangaSupa, SenManga };
        public static IRepository[] AllRepositories => allRepositories;

        public static ISeries GetSeriesFromData(Uri uri, string title)
        {
            ISeries output = null;
            foreach (var i in AllRepositories)
            {
                output = i.GetSeriesFromData(uri, title);
                if (output != null)
                {
                    return output;
                }
            }

            return output;
        }
    }
}
