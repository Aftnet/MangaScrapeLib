using MangaScrapeLib.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class Source
    {
        public static readonly Source EatManga = new Source(new EatMangaRepository());
        public static readonly Source MangaHere = new Source(new EatMangaRepository());

        protected readonly HttpClient Client = new HttpClient();
        internal readonly IRepository Repository;

        internal Source(IRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<Series>> GetDefaultSeries()
        {
            var html = await Client.GetStringAsync(Repository.MangaIndexPage);
            var series = Repository.GetDefaultSeries(html);
            return series;
        }
    }
}
