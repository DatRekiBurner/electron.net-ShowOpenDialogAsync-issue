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
using System.Threading;
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

                    string path = (await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options)).First();
                    Electron.IpcMain.Send(mainWindow, "select-directory-reply", path);
                });
            }

            return View();
        }

        [HttpPost]
        public IActionResult Download()
        {
            DownloadModel model = new()
            {
                Url = new Uri(Request.Form["download-url"].ToString(), UriKind.Absolute),
                Path = Request.Form["location"].ToString(),
                SelectHttpMethod = Request.Form["request-type"].ToString()
            };

            List<string> headerKeys = Request.Form["header-key"].ConvertToList();
            List<string> headerValues = Request.Form["header-value"].ConvertToList();

            // Add header keys and values into a single dictionary.
            if (headerKeys.Count > 0 && headerValues.Count > 0)
                model.Headers = Enumerable.Zip(headerKeys, headerValues).ToDictionary(x => x.First, x => x.Second);

            // Set a custom timeout duration if the user added any.
            if (int.TryParse(Request.Form["timeout"].ToString(), out int time))
                model.Timeout = TimeSpan.FromSeconds(time);

            Thread thread = new(() => Core.Download.Start(model));
            thread.Start();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
