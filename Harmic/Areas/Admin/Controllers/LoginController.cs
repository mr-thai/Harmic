using Harmic.Models;
using Harmic.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Harmic.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        public readonly HarmicContext _context;
        public LoginController(HarmicContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Index(TbAccount account)
        {
            if (account == null || string.IsNullOrEmpty(account.Password))
            {
                Function._Message = "Username and password are required";
                return View();
            }
            string password = HashMD5.GetMD5(account.Password);
            var check = _context.TbAccounts
                .Where(m => m.Username == account.Username && m.Password == password)
                .FirstOrDefault();

            if (check == null) {
                Function._Message = "Invalid Username or password";
                return View();
            }

            Function._Message = string.Empty;
            Function._AccountId = check.AccountId;
            Function._Username = check.Username;
            return RedirectToAction("Index", "Home");
        }
    }
}
