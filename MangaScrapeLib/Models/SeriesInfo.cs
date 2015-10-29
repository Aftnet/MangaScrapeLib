using MangaScrapeLib.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaScrapeLib.Models
{
    public class SeriesInfo : IPathSuggester
    {
        public const string PathSeparator = @"\";

        public readonly IMangaRepository ParentRepository;
        public readonly Uri SeriesPageUri;

        public string Name { get; set; }

        public string Tags { get; set; }
        public string Description { get; set; }

        public SeriesInfo(IMangaRepository Parent, Uri SeriesPageUri)
        {
            ParentRepository = Parent;
            this.SeriesPageUri = SeriesPageUri;
        }

        public async Task<IEnumerable<ChapterInfo>> GetChaptersAsync(bool ParseHtmlAsynchronously = true)
        {
            using (var Client = new HttpClient())
            {
                var PageHtml = await Client.GetStringAsync(SeriesPageUri);
                if (ParseHtmlAsynchronously)
                {
                    return await Task.Run(() => ParentRepository.GetChapters(this, PageHtml));
                }

                var Output = ParentRepository.GetChapters(this, PageHtml);
                return Output;
            }
        }

        public string SuggestPath(string RootDirectoryPath)
        {
            var Output = string.Format("{0}{1}{2}", RootDirectoryPath, PathSeparator, Name);
            Output = MakeValidPathName(Output);
            return Output;
        }

        public static string MakeValidPathName(string Name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);
            Name = Regex.Replace(Name, invalidReStr, " ");
            Name = Regex.Replace(Name, @"\s{2,}", " ");
            Name = Regex.Replace(Name, @"^[\s]+", "");
            Name = Regex.Replace(Name, @"[\s]+$", "");
            Name = Regex.Replace(Name, @"[\s]+", " ");
            return Name;
        }

    }
}
