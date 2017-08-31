using MangaScrapeLib.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories.Components
{
    internal abstract class NoFullListSearcher : ISearcher
    {
        protected abstract Uri GenerateSearchUri(string query);
        protected abstract ISeries[] ParseResultsString(string resultsString);

        private readonly Uri ReferreringUri;

        public NoFullListSearcher(Uri referreringUri)
        {
            ReferreringUri = referreringUri;
        }

        public async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var lowercaseQuery = query.ToLowerInvariant();
            var searchUri = GenerateSearchUri(lowercaseQuery);

            var receivedString = await WebClient.GetStringAsync(searchUri, ReferreringUri);
            var output = ParseResultsString(receivedString);
            output = output.OrderBy(d => d.Title).ToArray();
            return output;
        }
    }
}
