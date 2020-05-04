using MangaScrapeLib.Models;
using MangaScrapeLib.Repository;
using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MangaScrapeLib
{
    public static class Repositories
    {
        private static IDictionary<string, IRepository> HostToRepoDictionary { get; } = new Dictionary<string, IRepository>();
        public static IReadOnlyList<IRepository> AllRepositories { get; }

        static Repositories()
        {
            var client = new WebClient();
            var allRepositories = new List<IRepository>();

            void AddToDictionary(IRepository repo)
            {
                allRepositories.Add(repo);

                var key = repo.RootUri.Host;
                if (!HostToRepoDictionary.ContainsKey(key))
                {
                    HostToRepoDictionary.Add(repo.RootUri.Host, repo);
                }
            }

            //AddToDictionary(new EatMangaRepository(client));
            //AddToDictionary(new MangaDexRepository(client));
            AddToDictionary(new MangaEdenEnRepository(client));
            AddToDictionary(new MangaEdenItRepository(client));
            //AddToDictionary(new MangaKakalotRepository(client));
            AddToDictionary(new MangaNelRepository(client));
            AddToDictionary(new MangaStreamRepository(client));
            AddToDictionary(new MangaBatRepository(client));
            AddToDictionary(new SenMangaRepository(client));

            AllRepositories = allRepositories.OrderBy(d => d.Name).ToArray();
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

            if (HostToRepoDictionary.ContainsKey(uri.Host))
            {
                if (HostToRepoDictionary[uri.Host] is RepositoryBase repository)
                {
                    var output = new Series(repository, uri, title);
                    return output;
                }
            }

            return null;
        }
    }
}
