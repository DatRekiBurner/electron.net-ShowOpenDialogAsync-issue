using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Downloader.Core.Extensions
{
    public static class General
    {
        /// <summary>
        /// Try and convert StringValues to a List<string>.
        /// </summary>
        public static List<string> ConvertToList(this StringValues values)
        {
            List<string> result = new();

            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(value);
            }

            return result;
        }

        /// <summary>
        /// Get media types from headers.
        /// </summary>
        /// <param name="ContentType"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static string GetMediaType(this MediaTypeHeaderValue? ContentType)
        {

            if (ContentType != null && ContentType.MediaType != null)
                return ContentType.MediaType; //.Replace('\', @"/");

            return string.Empty;
        }
    }
}
