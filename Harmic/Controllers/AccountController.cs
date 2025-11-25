using Harmic.Models;
using Harmic.Utilities;
using Harmic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Harmic.Controllers
{
    public class AccountController : Controller
    {
        private readonly HarmicContext _context;
        private readonly ICartService _cartService;

        public AccountController(HarmicContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string action, TbAccount account)
        {
            if (action == "login")
            {
                if (account == null || string.IsNullOrWhiteSpace(account.Email) || string.IsNullOrWhiteSpace(account.Password))
                {
                    ViewBag.Message = "nhap.";
                    return View(account);
                }

                string password = HashMD5.GetMD5(account.Password);
                var user = _context.TbAccounts.FirstOrDefault(n => n.Email == account.Email && n.Password == password);

                if (user == null)
                {
                    ViewBag.Message = "Sai";
                    return View(account);
                }

                Function._AccountId = user.AccountId;
                Function._Username = user.Username;
                Function._Message = user.Email;
                var roleId = user.RoleId ?? 2;
                HttpContext.Session.SetInt32("RoleId", roleId);
                _cartService.MergeSessionCartToUserAsync(user.AccountId).GetAwaiter().GetResult();

                switch (roleId)
                {
                    case 1:
                        HttpContext.Session.SetString("AdminShowSidebar", "true");
                        return RedirectToAction("Index", "Home", new { area = "Admin" });

                    case 2:
                        return RedirectToAction("Index", "Home");

                    case 1002:
                        HttpContext.Session.SetString("AdminShowSidebar", "false");
                        return RedirectToAction("Index", "Menus", new { area = "Admin" });

                    case 1003:
                        HttpContext.Session.SetString("AdminShowSidebar", "false");
                        return RedirectToAction("Index", "Products", new { area = "Admin" });

                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            else if (action == "register")
            {
                if (account == null || string.IsNullOrWhiteSpace(account.Email) || string.IsNullOrWhiteSpace(account.Password) || string.IsNullOrWhiteSpace(account.Username))
                {
                    ViewBag.Message = "nhap";
                    return View(account);
                }

                var user = _context.TbAccounts.FirstOrDefault(n => n.Email == account.Email);
                if (user != null)
                {
                    ViewBag.Message = "co roi";
                    return View(account);
                }

                account.Password = HashMD5.GetMD5(account.Password);
                account.IsActive = true;
                account.RoleId = 2;
                account.FullName = account.Username;
                _context.TbAccounts.Add(account);
                _context.SaveChanges();
                ViewBag.Message = "ok";
                return View();
            }

            return View();
        }
    }
}
