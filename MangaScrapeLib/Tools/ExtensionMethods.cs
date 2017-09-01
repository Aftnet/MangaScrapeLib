using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MangaScrapeLib.Tools
{
    /// <summary>
    /// Utility class to manipulate Urls
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Truncates the last URI segment.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The URI in string form without the last segment</returns>
        public static string TruncateLastUriSegment(this string uri)
        {
            var tempUri = new Uri(uri);
            return tempUri.AbsoluteUri.Remove(tempUri.AbsoluteUri.Length - tempUri.Segments.Last().Length);
        }

        /// <summary>
        /// Gets the last URI segment.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The last URI segment as a string</returns>
        public static string GetLastUriSegment(this string uri)
        {
            return uri.Split('/').Last();
        }

        public static string MakeValidPathName(this string name)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);
            name = Regex.Replace(name, invalidReStr, " ");
            name = Regex.Replace(name, @"\s{2,}", " ");
            name = Regex.Replace(name, @"^[\s]+", "");
            name = Regex.Replace(name, @"[\s]+$", "");
            name = Regex.Replace(name, @"[\s]+", " ");
            return name;
        }
    }
}
