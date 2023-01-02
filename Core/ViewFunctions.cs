using Microsoft.AspNetCore.Http;

namespace Downloader.Core
{
    public class ViewFunctions
    {
        /// <summary>
        /// Get the base url from the current website/app.
        /// </summary>
        /// <param name="request">The HttpRequest that is provided by the controller</param>
        /// <returns>String containing the url to the website (scheme + host)</returns>
        public static string GetBaseUrl(HttpRequest request) => $"{request.Scheme}://{request.Host}";
    }
}
