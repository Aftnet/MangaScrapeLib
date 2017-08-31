using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories.Components
{
    internal abstract class FullListSearcher : ISearcher
    {
        protected abstract Task<ISeries[]> GetSeriesListAsync();

        private ISeries[] SeriesList = null;
        private bool SearchInProgress = false;
        private string CurrentQuery = null;

        public async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            CurrentQuery = query.ToLowerInvariant();
            if (SearchInProgress)
            {
                return new ISeries[0];
            }

            if (SeriesList == null)
            {
                SearchInProgress = true;
                SeriesList = await GetSeriesListAsync();
                SearchInProgress = false;
            }

            var output = SeriesList.Where(d => d.Title.ToLowerInvariant().Contains(CurrentQuery)).OrderBy(d => d.Title).ToArray();
            return output;
        }
    }
}
