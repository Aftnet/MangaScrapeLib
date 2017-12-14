using MangaScrapeLib.Tools;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MangaScrapeLib.Test.Tools
{
    public class WebCache : IWebClient
    {
        private readonly IWebClient Client = new WebClient();

        private readonly Dictionary<Uri, byte[]> ByteCache = new Dictionary<Uri, byte[]>();
        private readonly Dictionary<Uri, string> StringCache = new Dictionary<Uri, string>();

        public async Task<byte[]> GetByteArrayAsync(Uri uri, Uri referrer)
        {
            if (!ByteCache.ContainsKey(uri))
            {
                var data = await Client.GetByteArrayAsync(uri, referrer);
                ByteCache[uri] = data;
            }

            return ByteCache.GetValueOrDefault(uri);
        }

        public async Task<string> GetStringAsync(Uri uri, Uri referrer)
        {
            if (!StringCache.ContainsKey(uri))
            {
                var data = await Client.GetStringAsync(uri, referrer);
                StringCache[uri] = data;
            }

            return StringCache.GetValueOrDefault(uri);
        }

        public Task<HttpResponseMessage> PostAsync(HttpContent content, Uri uri, Uri referrer)
        {
            return Client.PostAsync(content, uri, referrer);
        }
    }
}
