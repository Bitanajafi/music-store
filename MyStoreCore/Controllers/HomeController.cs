using Microsoft.AspNetCore.Mvc;
using MyStoreCore.Models;
using System.Diagnostics;

namespace MyStoreCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }
    }
}
