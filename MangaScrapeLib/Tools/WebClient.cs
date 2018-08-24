using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScrapeLib.Tools
{
    internal class WebClient : IWebClient
    {
        protected static HttpClient Client { get; }

        static WebClient()
        {
            Client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            });

            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Android 8.1.0; Tablet; rv:59.0) Gecko/59.0 Firefox/59.0");
            Client.DefaultRequestHeaders.Add("Accept", "text/html, application/xhtml+xml, application/json, text/javascript, */*");
            Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        }

        public async Task<string> GetStringAsync(Uri uri, Uri referrer, CancellationToken token)
        {
            var output = default(string);
            Client.DefaultRequestHeaders.Referrer = referrer;
            var result = await Client.GetAsync(uri, token);
            if (result.IsSuccessStatusCode && !token.IsCancellationRequested)
            {
                output = await result.Content.ReadAsStringAsync();
            }

            return output;
        }

        public async Task<byte[]> GetByteArrayAsync(Uri uri, Uri referrer, CancellationToken token)
        {
            var output = default(byte[]);
            Client.DefaultRequestHeaders.Referrer = referrer;
            var result = await Client.GetAsync(uri, token);
            if (result.IsSuccessStatusCode && !token.IsCancellationRequested)
            {
                output = await result.Content.ReadAsByteArrayAsync();
            }

            return output;
        }

        public Task<HttpResponseMessage> PostAsync(HttpContent content, Uri uri, Uri referrer, CancellationToken token)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.PostAsync(uri, content, token);
        }
    }
}
