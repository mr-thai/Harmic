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
                    ViewBag.Message = "Vui lòng nhập email và mật khẩu.";
                    return View(account);
                }

                string password = HashMD5.GetMD5(account.Password);
                var user = _context.TbAccounts.FirstOrDefault(n => n.Email == account.Email && n.Password == password);

                if (user == null)
                {
                    ViewBag.Message = "Sai email hoặc mật khẩu.";
                    return View(account);
                }

                Function._AccountId = user.AccountId;
                Function._Username = user.Username ?? string.Empty;
                Function._Message = user.Email ?? string.Empty;
                HttpContext.Session.SetInt32("RoleId", user.RoleId ?? 0);

                // Merge any session cart into this user cart
                _cartService.MergeSessionCartToUserAsync(user.AccountId).GetAwaiter().GetResult();

                if (user.RoleId == 1)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else if (action == "register")
            {
                if (account == null || string.IsNullOrWhiteSpace(account.Email) || string.IsNullOrWhiteSpace(account.Password) || string.IsNullOrWhiteSpace(account.Username))
                {
                    ViewBag.Message = "Vui lòng nhập đầy đủ thông tin.";
                    return View(account);
                }

                var user = _context.TbAccounts.FirstOrDefault(n => n.Email == account.Email);
                if (user != null)
                {
                    ViewBag.Message = "Email đã tồn tại.";
                    return View(account);
                }

                account.Password = HashMD5.GetMD5(account.Password);
                account.IsActive = true;
                account.RoleId = 2;
                account.FullName = account.Username;
                _context.TbAccounts.Add(account);
                _context.SaveChanges();
                ViewBag.Message = "Đăng ký thành công.";
                return View();
            }

            return View();
        }
    }
}
