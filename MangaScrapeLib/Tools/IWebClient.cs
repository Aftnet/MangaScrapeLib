using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaScrapeLib.Tools
{
    internal interface IWebClient
    {
        Task<byte[]> GetByteArrayAsync(Uri uri, Uri referrer);
        Task<string> GetStringAsync(Uri uri, Uri referrer);
        Task<HttpResponseMessage> PostAsync(HttpContent content, Uri uri, Uri referrer);
    }
}