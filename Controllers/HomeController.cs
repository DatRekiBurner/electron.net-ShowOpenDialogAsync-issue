using Downloader.Core.Extensions;
using Downloader.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http.Handlers;

namespace Downloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static readonly HttpClientHandler _handler = new() { AllowAutoRedirect = true };
        private static readonly ProgressMessageHandler _progressHandler = new(_handler);
        private static readonly HttpClient _client = new(_progressHandler);

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Download()
        {
            string url = Request.Form["download-url"].ToString();

            Dictionary<string, string> headers = new();
            List<string> headerKeys = Request.Form["header-key"].ConvertToList();
            List<string> headerValues = Request.Form["header-value"].ConvertToList();
            string selectHttpMethod = Request.Form["request-type"].ToString();
            TimeSpan timeout = TimeSpan.FromSeconds(100);

            if (headerKeys.Count > 0 && headerValues.Count > 0)
                headers = Enumerable.Zip(headerKeys, headerValues).ToDictionary(x => x.First, x => x.Second);

            HttpMethod httpMethod = selectHttpMethod switch
            {
                "Post" => HttpMethod.Post,
                "Put" => HttpMethod.Put,
                "Patch" => HttpMethod.Patch,
                "Delete" => HttpMethod.Delete,
                _ => HttpMethod.Get,
            };

            using HttpRequestMessage requestMessage = new(httpMethod, url);

            // Add headers if user passed any via the form.
            if (headers.Count > 0)
                foreach (KeyValuePair<string, string> header in headers)
                    requestMessage.Headers.Add(header.Key, header.Value);

            // Set a custom timeout duration if the user added any.
            if (int.TryParse(Request.Form["timeout"].ToString(), out int time))
                timeout = TimeSpan.FromSeconds(time);

            // Get progress from download/upload.
            _progressHandler.HttpSendProgress += new EventHandler<HttpProgressEventArgs>(HttpProgress);
            _progressHandler.HttpReceiveProgress += new EventHandler<HttpProgressEventArgs>(HttpProgress);

            // Set timeout duration.
            _client.Timeout = timeout;

            // Send request.
            HttpResponseMessage result = await _client.SendAsync(requestMessage);

            // Figure out what extension to use for the content we're about to download.
            string extension = result.Content.Headers.ContentType.GetMediaType() switch
            {
                @"image\jpeg" => ".jpg",
                _ => ".file",
            };

            // Path for where we download the file
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString(), $"download{extension}");
            // Write downloaded content to the file
            System.IO.File.WriteAllText(path, await result.Content.ReadAsStringAsync());


            // To do:
            // Make download happen in a seperate thread.
            // Add more extensions based on MIME type `https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types`.
            // Allow user to select location the file is saved to.
            // Implement progress bar with `https://www.jsdelivr.com/package/npm/@loadingio/loading-bar`.
            // Update progress bar with Ajax.

            return View();
        }

        private void HttpProgress(object? sender, HttpProgressEventArgs e)
        {
            Console.WriteLine($"progress: {(double)e.BytesTransferred / e.TotalBytes}");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}