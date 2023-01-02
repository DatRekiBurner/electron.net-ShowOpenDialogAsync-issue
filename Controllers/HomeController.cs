using Downloader.Models;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;

namespace Downloader.Controllers
{
    public class HomeController : Controller
    {
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

                    string[] paths = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                    Electron.IpcMain.Send(mainWindow, "select-directory-reply", paths);
                });
            }

            return View();
        }

        [HttpPost]
        public IActionResult Test()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
