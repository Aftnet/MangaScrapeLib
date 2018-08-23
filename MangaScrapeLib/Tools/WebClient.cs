using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaScrapeLib.Tools
{
    internal class WebClient : IWebClient
    {
        protected static readonly HttpClient Client;

        static WebClient()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Android 8.1.0; Tablet; rv:59.0) Gecko/59.0 Firefox/59.0");
            Client.DefaultRequestHeaders.Add("Accept", "text/html, application/xhtml+xml, */*");
        }

        public Task<string> GetStringAsync(Uri uri, Uri referrer)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.GetStringAsync(uri);
        }

        public Task<byte[]> GetByteArrayAsync(Uri uri, Uri referrer)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.GetByteArrayAsync(uri);
        }

        public Task<HttpResponseMessage> PostAsync(HttpContent content, Uri uri, Uri referrer)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.PostAsync(uri, content);
        }
    }
}
