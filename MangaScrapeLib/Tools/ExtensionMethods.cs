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
