using MangaScrapeLib.Models;
using MangaScrapeLib.Tools;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repository
{
    internal class MangaNelRepository : RepositoryBase
    {
        [JsonObject]
        private class SearchEntry
        {
            [JsonProperty("name")]
            public string DisplayName { get; set; }
            [JsonProperty("nameunsigned")]
            public string UriBase { get; set; }
            [JsonProperty("image")]
            public string CoverUri { get; set; }
            [JsonProperty("author")]
            public string Author { get; set; }
        }

        private const string SearchUriPattern = "http://manganel.com/home/getjsonsearchstory?searchword={0}&search_style=tentruyen";
        private const string ReadUriPattern = "http://manganel.com/manga/{0}";
        private static readonly string[] SupuriousTitleText = { "<span style=\"color: #FF530D;font-weight: bold;\">", "</span>" };

        public MangaNelRepository() : base("Manga nel", "http://manganel.com/", "MangaNel.png", true)
        {
        }

        public override async Task<ISeries[]> SearchSeriesAsync(string query)
        {
            var uriQuery = Uri.EscapeDataString(query);
            var searchUri = new Uri(string.Format(SearchUriPattern, uriQuery));

            var json = await WebClient.GetStringAsync(searchUri, RootUri);
            var searchResult = JsonConvert.DeserializeObject<SearchEntry[]>(json);

            var output = searchResult.Select(d =>
            {
                var uri = new Uri(string.Format(ReadUriPattern, d.UriBase));
                var title = d.DisplayName;
                foreach(var i in SupuriousTitleText)
                {
                    title = title.Replace(i, string.Empty);
                }

                return new Series(this, uri, title)
                {
                    Author = d.Author,
                    CoverImageUri = new Uri(RootUri, d.CoverUri)
                };
            }).OrderBy(d => d.Title).ToArray();

            return output;
        }

        internal override Task<IChapter[]> GetChaptersAsync(ISeries input)
        {
            throw new NotImplementedException();
        }

        internal override Task<IPage[]> GetPagesAsync(IChapter input)
        {
            throw new NotImplementedException();
        }

        internal override Task<byte[]> GetImageAsync(IPage input)
        {
            throw new NotImplementedException();
        }
    }
}
