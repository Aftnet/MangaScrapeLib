using MangaScrapeLib.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    public abstract class NoFullListRepositoryBase : RepositoryBase
    {
        protected abstract Uri GetSearchUri(string query);
        protected abstract Series[] GetSeriesFromSearch(string searchPageHtml);

        public NoFullListRepositoryBase(string name, string uriString, string mangaIndexPageStr) : base(name, uriString, mangaIndexPageStr)
        {
        }

        public override async Task<Series[]> SearchSeriesAsync(string query)
        {
            var lowercaseQuery = query.ToLowerInvariant();
            var searchUri = GetSearchUri(lowercaseQuery);

            var html = await Client.GetStringAsync(searchUri);
            var output = GetSeriesFromSearch(html);
            output = output.OrderBy(d => d.Name).ToArray();
            return output;
        }
    }
}
