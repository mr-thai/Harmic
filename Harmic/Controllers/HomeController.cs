using System.Diagnostics;
using Harmic.Models;
using Microsoft.AspNetCore.Mvc;

namespace Harmic.Controllers
{
    public class HomeController : Controller
    {
        private readonly HarmicContext _context;
        private readonly ILogger<HomeController> _logger;
        public IActionResult Index()
        {
            ViewBag.productCategories = _context.TbProductCategories.ToList();
            ViewBag.productNew = _context.TbProducts.Where(m=>m.IsNew==true).ToList();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public HomeController(ILogger<HomeController> logger, HarmicContext context)
        {
            _logger = logger;
            _context = context;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
