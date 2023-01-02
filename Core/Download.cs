using System;
using System.Net.Http.Handlers;
using System.Net.Http;
using Downloader.Models;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using Downloader.Core.Extensions;
using MimeTypes;
using System.IO;
using System.Threading;

namespace Downloader.Core
{
    public class Download
    {
        public static async void Start(DownloadModel model)
        {
            Console.WriteLine($"Url: {model.Url}\n" +
                $"Timeout: {model.Timeout}\n" +
                $"SelectHttpMethod: {model.SelectHttpMethod}\n" +
                $"Path: {model.Path}\n" +
                $"Headers count: {model.Headers.Count}");

            if (model.Url == null)
                return;

            string path = model.Path;

            HttpClientHandler handler = new() { AllowAutoRedirect = true };
            ProgressMessageHandler progressHandler = new(handler);
            using HttpClient client = new(progressHandler);

            HttpMethod httpMethod = model.SelectHttpMethod switch
            {
                "Post" => HttpMethod.Post,
                "Put" => HttpMethod.Put,
                "Patch" => HttpMethod.Patch,
                "Delete" => HttpMethod.Delete,
                _ => HttpMethod.Get,
            };

            using HttpRequestMessage requestMessage = new(httpMethod, model.Url);

            // Add headers if user passed any via the form.
            if (model.Headers.Count > 0)
                foreach (KeyValuePair<string, string> header in model.Headers)
                    requestMessage.Headers.Add(header.Key, header.Value);

            // Get progress from download/upload.
            progressHandler.HttpSendProgress += new EventHandler<HttpProgressEventArgs>(Progress);
            progressHandler.HttpReceiveProgress += new EventHandler<HttpProgressEventArgs>(Progress);

            // Set timeout duration.
            client.Timeout = model.Timeout;

            // Send request.
            HttpResponseMessage result = await client.SendAsync(requestMessage);

            // Figure out what extension to use for the content we're about to download.
            string extension = MimeTypeMap.GetExtension(result.Content.Headers.ContentType.GetMediaType(), false);
            if (string.IsNullOrEmpty(extension))
                extension = ".file";

            // Update path to include filename/extension.
            path = Path.Combine(path, Path.GetFileNameWithoutExtension(model.Url.LocalPath) + extension);

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
        }

        private static void Progress(object? sender, HttpProgressEventArgs e)
        {
            Console.WriteLine($"progress: {(double)e.BytesTransferred / e.TotalBytes}");
        }
    }
}
