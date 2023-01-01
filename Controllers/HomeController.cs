using Downloader.Core.Extensions;
using Downloader.Models;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Threading.Tasks;

namespace Downloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HybridSupport.IsElectronActive)
            {
                Electron.IpcMain.On("select-directory", async (args) =>
                {
                    BrowserWindow mainWindow = Electron.WindowManager.BrowserWindows.First();
                    OpenDialogOptions options = new()
                    {
                        Properties = new OpenDialogProperty[]
                        {
                            OpenDialogProperty.openDirectory
                        }
                    };

                    string[] path = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                    Electron.IpcMain.Send(mainWindow, "select-directory-reply", path);
                });
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Download()
        {
            Uri url = new(Request.Form["download-url"].ToString(), UriKind.Absolute);
            // The path of where the file will be saved.
            string path = Request.Form["location"].ToString();
            Console.WriteLine($"Path: {path}");

            HttpClientHandler handler = new() { AllowAutoRedirect = true };
            ProgressMessageHandler progressHandler = new(handler);
            using HttpClient client = new(progressHandler);

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
            progressHandler.HttpSendProgress += new EventHandler<HttpProgressEventArgs>(HttpProgress);
            progressHandler.HttpReceiveProgress += new EventHandler<HttpProgressEventArgs>(HttpProgress);

            // Set timeout duration.
            client.Timeout = timeout;

            // Send request.
            HttpResponseMessage result = await client.SendAsync(requestMessage);

            // Figure out what extension to use for the content we're about to download.
            string extension = MimeTypeMap.GetExtension(result.Content.Headers.ContentType.GetMediaType(), false);
            if (string.IsNullOrEmpty(extension))
                extension = ".file";

            // Update path to include filename/extension.
            path = Path.Combine(path, Path.GetFileNameWithoutExtension(url.LocalPath) + extension);

            // Create stream from content
            using Stream content = await result.Content.ReadAsStreamAsync();
            // Create new file stream.
            using FileStream file = new(path, FileMode.Create);
            // Steam content to file.
            await content.CopyToAsync(file);

            Console.WriteLine($"Downloaded file to: {path}");

            // To do:
            // Make download happen in a seperate thread.
            // Add more extensions based on MIME type `https://www.iana.org/assignments/media-types/media-types.xhtml`.
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
