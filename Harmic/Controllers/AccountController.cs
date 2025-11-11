using Harmic.Models;
using Harmic.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Harmic.Controllers
{
    public class AccountController : Controller
    {
        private readonly HarmicContext _context;
        public AccountController(HarmicContext context)
        {
            _context = context;
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

                if (user.RoleId == 1)
                {
                    // Admin: chuyển đến trang chủ admin
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }
                else
                {
                    // Người dùng thường: chuyển đến trang chủ
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
