using Harmic.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Harmic.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        [Area("Admin")]
        public IActionResult Index()
        {
            if (!Function.IsLogin())
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public IActionResult Logout()
        {
            Function._AccountId = 0;
            Function._Username = string.Empty;
            Function._Message = string.Empty;
            return RedirectToAction("Index", "Login");
        }
    }
}
