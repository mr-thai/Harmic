using Harmic.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Harmic.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart)
        {
            _cart = cart;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = await _cart.GetOrCreateCartAsync();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
        {
            await _cart.AddItemAsync(productId, quantity);
            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            await _cart.UpdateItemAsync(productId, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            await _cart.RemoveItemAsync(productId);
            return RedirectToAction(nameof(Index));
        }

        // --- AJAX endpoints ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAjax(int productId, int quantity = 1)
        {
            await _cart.AddItemAsync(productId, quantity);
            var cart = await _cart.GetCartAsync();
            var itemCount = await _cart.GetItemCountAsync();
            return Json(new
            {
                success = true,
                itemCount,
                totalAmount = cart?.TotalAmount ?? 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAjax(int productId, int quantity)
        {
            await _cart.UpdateItemAsync(productId, quantity);
            var cart = await _cart.GetCartAsync();
            var line = cart?.TbOrderDetails.FirstOrDefault(d => d.ProductId == productId);
            var lineTotal = (int)((line?.Price ?? 0m) * (line?.Quantity ?? 0));
            var itemCount = await _cart.GetItemCountAsync();

            return Json(new
            {
                success = true,
                lineTotal,
                totalAmount = cart?.TotalAmount ?? 0,
                itemCount
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAjax(int productId)
        {
            await _cart.RemoveItemAsync(productId);
            var cart = await _cart.GetCartAsync();
            var itemCount = await _cart.GetItemCountAsync();
            return Json(new
            {
                success = true,
                totalAmount = cart?.TotalAmount ?? 0,
                itemCount
            });
        }

        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            var count = await _cart.GetItemCountAsync();
            return Json(new { itemCount = count });
        }
    }
}