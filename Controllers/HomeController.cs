using Downloader.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            return View();
        }

        [HttpPost]
        public IActionResult Download()
        {
            string url = Request.Form["download-url"].ToString();

            // To do:
            // Download file from 'url' with HttpClient.
            // Allow user to select location the file is saved to.
            // Implement progress bar with `https://www.jsdelivr.com/package/npm/@loadingio/loading-bar`.
            // Update progress bar with Ajax.

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}