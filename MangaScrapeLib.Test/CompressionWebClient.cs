using System;
using System.ComponentModel;
using System.Net;

namespace MangaScrapeLib.Test
{
    [DesignerCategory("Code")]
    public class CompressionWebClient : WebClient
    {
        public CompressionWebClient()
            : base()
        {
            Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:17.0) Gecko/20100101 Firefox/17.0";
            Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }
    }
}
