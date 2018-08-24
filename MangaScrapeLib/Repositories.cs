using MangaScrapeLib.Models;
using MangaScrapeLib.Repository;
using MangaScrapeLib.Tools;
using System;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MangaScrapeLib.Test")]

namespace MangaScrapeLib
{
    public static class Repositories
    {
        public static IRepository EatManga { get; }
        public static IRepository MangaDex { get; }
        public static IRepository MangaEdenEn { get; }
        public static IRepository MangaEdenIt { get; }
        public static IRepository MangaKakalot { get; }
        public static IRepository MangaNel { get; }
        public static IRepository MangaStream { get; }
        public static IRepository MangaSupa { get; }
        public static IRepository SenManga { get; }

        public static IRepository[] AllRepositories { get; }

        static Repositories()
        {
            var client = new WebClient();

            EatManga = new EatMangaRepository(client);
            MangaDex = new MangaDexRepository(client);
            MangaEdenEn = new MangaEdenEnRepository(client);
            MangaEdenIt = new MangaEdenItRepository(client);
            MangaKakalot = new MangaKakalotRepository(client);
            MangaNel = new MangaNelRepository(client);
            MangaStream = new MangaStreamRepository(client);
            MangaSupa = new MangaBatRepository(client);
            SenManga = new SenMangaRepository(client);

            AllRepositories = new IRepository[] { EatManga, MangaDex, MangaEdenEn, MangaEdenIt, MangaKakalot, MangaNel, MangaStream, MangaSupa, SenManga };
        }

        internal static IRepository DetermineOwnerRepository(Uri uri)
        {
            return AllRepositories.FirstOrDefault(d => d.RootUri.Host == uri.Host);
        }

        public static ISeries GetSeriesFromData(Uri uri, string title)
        {
            if (uri == null || string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var repository = DetermineOwnerRepository(uri) as RepositoryBase;
            if (repository == null)
            {
                return null;
            }

            var output = new Series(repository, uri, title);
            return output;
        }
    }
}
