using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaScrapeLib.Tools
{
    internal class WebClient
    {
        protected static readonly HttpClient Client;

        static WebClient()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:33.0) Gecko/20100101 Firefox/33.0");
        }

        public static Task<string> GetStringAsync(Uri uri, Uri referrer)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.GetStringAsync(uri);
        }

        public static Task<byte[]> GetByteArrayAsync(Uri uri, Uri referrer)
        {
            Client.DefaultRequestHeaders.Referrer = referrer;
            return Client.GetByteArrayAsync(uri);
        }
    }
}
