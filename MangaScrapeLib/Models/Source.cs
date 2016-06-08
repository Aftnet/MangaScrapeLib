using MangaScrapeLib.Repositories;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class Source
    {
        public static readonly Source EatManga = new Source(new EatMangaRepository());
        public static readonly Source MangaHere = new Source(new MangaHereRepository());
        public static readonly Source[] AllSources = new Source[] { EatManga, MangaHere };

        protected static readonly HttpClient Client = new HttpClient();
        internal readonly IRepository Repository;

        private Series[] DefaultSeries = null;

        internal Source(IRepository repository)
        {
            Repository = repository;
        }

        public async Task<Series[]> GetSeriesAsync()
        {
            if (DefaultSeries != null) return DefaultSeries;

            var html = await Client.GetStringAsync(Repository.MangaIndexPage);
            DefaultSeries = Repository.GetDefaultSeries(this, html);
            DefaultSeries.OrderBy(d => d.Name).ToArray();
            return DefaultSeries;
        }

        public async Task<Series[]> SearchSeriesAsync(string query)
        {
            var lowercaseQuery = query.ToLowerInvariant();

            var searchableRepository = Repository as ISearchableRepository;
            if(searchableRepository == null)
            {
                var series = await GetSeriesAsync();
                return series.Where(d => d.Name.ToLowerInvariant().Contains(lowercaseQuery)).ToArray();
            }

            var searchUri = searchableRepository.GetSearchUri(lowercaseQuery);
            var html = await Client.GetStringAsync(searchUri);
            var output = searchableRepository.GetSeriesFromSearch(this, html);
            output = output.OrderBy(d => d.Name).ToArray();
            return output;
        }

        static internal async Task<Chapter[]> GetChaptersAsync(Series input)
        {
            var html = await Client.GetStringAsync(input.SeriesPageUri);
            var output = input.ParentSource.Repository.GetChapters(input, html);
            return output;
        }

        static internal async Task<Page[]> GetPagesAsync(Chapter input)
        {
            var html = await Client.GetStringAsync(input.FirstPageUri);
            var output = input.ParentSeries.ParentSource.Repository.GetPages(input, html);
            return output;
        }

        static internal async Task<byte[]> GetImageAsync(Page input)
        {
            var html = await Client.GetStringAsync(input.PageUri);
            input.ImageUri = input.ParentChapter.ParentSeries.ParentSource.Repository.GetImageUri(html);
            var output = await Client.GetByteArrayAsync(input.ImageUri);
            return output;
        }
    }
}
