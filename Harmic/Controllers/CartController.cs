using Harmic.Models;
using Harmic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Harmic.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cart;
        private readonly HarmicContext _db;

        public CartController(ICartService cart, HarmicContext db)
        {
            _cart = cart;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Hiển thị tất cả đơn hàng có trạng thái = 1, không ràng buộc AccountId
            const int activeStatusId = 1;
            var orders = await _db.TbOrders
                .Include(o => o.TbOrderDetails)
                .Where(o => o.OrderStatusId == activeStatusId)
                .ToListAsync();

            var aggregatedDetails = new List<TbOrderDetail>();
            foreach (var o in orders)
            {
                aggregatedDetails.AddRange(o.TbOrderDetails);
            }

            var productIds = aggregatedDetails
                .Where(d => d.ProductId.HasValue)
                .Select(d => d.ProductId!.Value)
                .Distinct()
                .ToList();

            var productMap = await _db.TbProducts
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            var totalAmount = (int)aggregatedDetails.Sum(d => (d.Price ?? 0m) * (d.Quantity ?? 0));
            var totalQty = aggregatedDetails.Sum(d => d.Quantity ?? 0);

            // Tạo model TbOrder "tổng hợp" để tái sử dụng view hiện tại
            var model = new TbOrder
            {
                TbOrderDetails = aggregatedDetails,
                TotalAmount = totalAmount,
                Quanlity = totalQty
            };

            ViewBag.ProductMap = productMap;
            return View(model);
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

        // MiniCart dữ liệu cho header (realtime)
        [HttpGet]
        public async Task<IActionResult> MiniCart()
        {
            var cart = await _cart.GetCartAsync() ?? await _cart.GetOrCreateCartAsync();
            // Giữ nguyên logic hiện tại để tránh lẫn dữ liệu người khác
            var ids = cart.TbOrderDetails
                .Where(d => d.ProductId.HasValue)
                .Select(d => d.ProductId!.Value)
                .Distinct()
                .ToList();

            var products = await _db.TbProducts
                .Where(p => ids.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            var items = cart.TbOrderDetails.Select(d =>
            {
                var pid = d.ProductId ?? 0;
                products.TryGetValue(pid, out var p);
                var price = (int)(d.Price ?? 0m);
                var qty = d.Quantity ?? 0;
                var lineTotal = price * qty;

                return new
                {
                    productId = pid,
                    title = p?.Title ?? $"Product #{pid}",
                    imageUrl = Url.Content("~/" + (string.IsNullOrWhiteSpace(p?.Image) ? "assets/images/product/small-size/1-1-112x124.jpg" : p!.Image)),
                    price,
                    quantity = qty,
                    lineTotal
                };
            }).ToList();

            var subtotal = items.Sum(i => i.lineTotal);
            var itemCount = items.Sum(i => i.quantity);

            return Json(new { itemCount, subtotal, items });
        }
    }
}