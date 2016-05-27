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

        public async Task<IEnumerable<PageInfo>> GetPagesAsync(bool ParseHtmlAsynchronously = true)
        {
            using (var Client = new HttpClient())
            {
                var PageHtml = await Client.GetStringAsync(FirstPageUri);
                if (ParseHtmlAsynchronously)
                {
                    return await Task.Run(() => ParentSeries.ParentRepository.GetPages(this, PageHtml));
                }

                var Output = ParentSeries.ParentRepository.GetPages(this, PageHtml);
                return Output;
            }
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
