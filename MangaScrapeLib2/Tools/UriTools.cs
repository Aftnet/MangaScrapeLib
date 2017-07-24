using System;
using System.Linq;

namespace MangaScrapeLib.Tools
{
    /// <summary>
    /// Utility class to manipulate Urls
    /// </summary>
    public static class UriTools
    {
        /// <summary>
        /// Truncates the last URI segment.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The URI in string form without the last segment</returns>
        public static string TruncateLastUriSegment(string uri)
        {
            var tempUri = new Uri(uri);
            return tempUri.AbsoluteUri.Remove(tempUri.AbsoluteUri.Length - tempUri.Segments.Last().Length);
        }

        /// <summary>
        /// Gets the last URI segment.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The last URI segment as a string</returns>
        public static string GetLastUriSegment(string uri)
        {
            return uri.Split('/').Last();
        }
    }
}
