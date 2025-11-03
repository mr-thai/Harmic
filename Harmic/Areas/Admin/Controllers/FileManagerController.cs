using Microsoft.AspNetCore.Mvc;

namespace Harmic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/Admin/file-manager")]
    public class FileManagerController : Controller
    {
        // GET: /Admin/FileManagerX
        public IActionResult Index()
        {
            return View();
        }
    }
}
