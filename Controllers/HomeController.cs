using DIP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DIP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return RedirectToAction("Home", "Knowledge");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleLayout()
        {
            var currentLayout = HttpContext.Session.GetString("Layout") ?? "layout1";
            var newLayout = currentLayout == "layout1" ? "layout2" : "layout1";

            HttpContext.Session.SetString("Layout", newLayout);

            // 可依需求導回上一頁或首頁
            return RedirectToAction("Home", "Knowledge"); // 或 Redirect(Request.Headers["Referer"].ToString());
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}
