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
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:33.0) Gecko/20100101 Firefox/33.0");
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
