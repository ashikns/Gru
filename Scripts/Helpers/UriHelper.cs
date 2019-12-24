using System;
using System.Text.RegularExpressions;

namespace Gru.Helpers
{
    public static class UriHelper
    {
        private const string DataUriPattern = @"data:image/(?<type>.+?),(?<data>.+)";

        public static bool TryParseDataUri(string dataUri, out byte[] data)
        {
            var match = Regex.Match(dataUri, DataUriPattern);

            if (match.Success)
            {
                data = Convert.FromBase64String(match.Groups["data"].Value);
                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }
    }
}