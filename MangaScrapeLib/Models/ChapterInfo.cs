using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class ChapterInfo : IPathSuggester
    {
        public readonly SeriesInfo ParentSeries;

        public readonly Uri FirstPageUri;

        public string Title { get; set; }

        public ChapterInfo(SeriesInfo Parent, Uri FirstPageUri)
        {
            ParentSeries = Parent;
            this.FirstPageUri = FirstPageUri;
        }

        public async Task<IEnumerable<PageInfo>> GetPagesAsync(HttpClient Client)
        {
            var PageHtml = await Client.GetStringAsync(FirstPageUri);
            var Output = ParentSeries.ParentRepository.GetPages(this, PageHtml);
            return Output;
        }

        public string SuggestPath(string RootDirectoryPath)
        {
            var ParentSeriesPath = ParentSeries.SuggestPath(RootDirectoryPath);
            var Output = Path.Combine(ParentSeries.SuggestPath(RootDirectoryPath), SeriesInfo.MakeValidPathName(Title));

            Output = SeriesInfo.MakeValidPathName(Output);
            return Output;
        }
    }
}
