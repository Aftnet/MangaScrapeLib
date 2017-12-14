using MangaScrapeLib.Repository;
using MangaScrapeLib.Tools;
using System;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MangaScrapeLib.Test")]

namespace MangaScrapeLib
{
    public static class Repositories
    {
        public static IRepository EatManga { get; private set; }

        public static IRepository MangaEden { get; private set; }

        //public static IRepository MangaHere => mangaHere;

        public static IRepository MangaKakalot { get; private set; }

        public static IRepository MangaNel { get; private set; }

        public static IRepository MangaStream { get; private set; }

        public static IRepository MangaSupa { get; private set; }

        //public static IRepository MyManga => myManga;

        public static IRepository SenManga { get; private set; }

        public static IRepository[] AllRepositories { get; private set; }

        static Repositories()
        {
            var client = new WebClient();

            EatManga = new EatMangaRepository(client);
            MangaEden = new MangaEdenRepository(client);
            MangaKakalot = new MangaKakalotRepository(client);
            MangaNel = new MangaNelRepository(client);
            MangaStream = new MangaStreamRepository(client);
            MangaSupa = new MangaSupaRepository(client);
            SenManga = new SenMangaRepository(client);

            AllRepositories = new IRepository[] { EatManga, MangaEden, MangaKakalot, MangaNel, MangaStream, MangaSupa, SenManga };
        }

        public static ISeries GetSeriesFromData(Uri uri, string title)
        {
            if (uri == null || string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var repository = DetermineOwnerRepository(uri);
            var output = repository?.GetSeriesFromData(uri, title);
            return output;
        }

        internal static IRepository DetermineOwnerRepository(Uri uri)
        {
            return AllRepositories.FirstOrDefault(d => d.RootUri.Host == uri.Host);
        }
    }
}
