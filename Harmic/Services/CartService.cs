using Harmic.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Harmic.Services
{
    public class CartService : ICartService
    {
        private const string SessionCartOrderIdKey = "CART_ORDER_ID";
        private readonly HarmicContext _db;
        private readonly IHttpContextAccessor _http;

        public CartService(HarmicContext db, IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }

        private ISession Session => _http.HttpContext!.Session;

        private int? CurrentAccountId
        {
            get
            {
                // Align with existing pattern (AccountController sets this)
                // If you prefer, you can also read from claims or session
                var type = Type.GetType("Harmic.Utilities.Function, Harmic");
                var prop = type?.GetProperty("_AccountId");
                if (prop == null) return null;
                var val = prop.GetValue(null);
                if (val is int id && id > 0) return id;
                return null;
            }
        }

        public async Task<TbOrder?> GetCartAsync()
        {
            var accountId = CurrentAccountId;
            if (accountId.HasValue)
            {
                var cartStatusId = await EnsureStatusAsync("Cart");
                return await _db.TbOrders
                    .Include(o => o.TbOrderDetails)
                    .FirstOrDefaultAsync(o => o.AccountId == accountId.Value && o.OrderStatusId == cartStatusId);
            }
            else
            {
                var orderId = Session.GetInt32(SessionCartOrderIdKey);
                if (orderId is null) return null;
                return await _db.TbOrders
                    .Include(o => o.TbOrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId.Value);
            }
        }

        public async Task<TbOrder> GetOrCreateCartAsync()
        {
            var accountId = CurrentAccountId;
            var cartStatusId = await EnsureStatusAsync("Cart");

            if (accountId.HasValue)
            {
                var cart = await _db.TbOrders
                    .Include(o => o.TbOrderDetails)
                    .FirstOrDefaultAsync(o => o.AccountId == accountId.Value && o.OrderStatusId == cartStatusId);

                if (cart != null) return cart;

                cart = new TbOrder
                {
                    AccountId = accountId.Value,
                    OrderStatusId = cartStatusId,
                    CreatedDate = DateTime.UtcNow,
                    Code = null,
                    Quanlity = 0,
                    TotalAmount = 0
                };
                _db.TbOrders.Add(cart);
                await _db.SaveChangesAsync();
                return cart;
            }
            else
            {
                var orderId = Session.GetInt32(SessionCartOrderIdKey);
                TbOrder? cart = null;

                if (orderId.HasValue)
                {
                    cart = await _db.TbOrders
                        .Include(o => o.TbOrderDetails)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId.Value && o.OrderStatusId == cartStatusId);
                }

                if (cart != null) return cart;

                cart = new TbOrder
                {
                    AccountId = null,
                    OrderStatusId = cartStatusId,
                    CreatedDate = DateTime.UtcNow,
                    Code = null,
                    Quanlity = 0,
                    TotalAmount = 0
                };
                _db.TbOrders.Add(cart);
                await _db.SaveChangesAsync();

                Session.SetInt32(SessionCartOrderIdKey, cart.OrderId);
                return cart;
            }
        }

        public async Task AddItemAsync(int productId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var cart = await GetOrCreateCartAsync();

            var product = await _db.TbProducts.FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);
            if (product == null) throw new InvalidOperationException("Product not found or inactive.");

            var price = (decimal)(product.PriceSale ?? product.Price ?? 0);

            var line = cart.TbOrderDetails.FirstOrDefault(d => d.ProductId == productId);
            if (line == null)
            {
                line = new TbOrderDetail
                {
                    OrderId = cart.OrderId,
                    ProductId = productId,
                    Price = price,
                    Quantity = quantity
                };
                _db.TbOrderDetails.Add(line);
            }
            else
            {
                line.Quantity = (line.Quantity ?? 0) + quantity;
            }

            await RecomputeTotalsAsync(cart);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync();
            var line = cart.TbOrderDetails.FirstOrDefault(d => d.ProductId == productId);
            if (line == null) return;

            if (quantity <= 0)
            {
                _db.TbOrderDetails.Remove(line);
            }
            else
            {
                line.Quantity = quantity;
            }

            await RecomputeTotalsAsync(cart);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(int productId)
        {
            var cart = await GetOrCreateCartAsync();
            var line = cart.TbOrderDetails.FirstOrDefault(d => d.ProductId == productId);
            if (line == null) return;

            _db.TbOrderDetails.Remove(line);
            await RecomputeTotalsAsync(cart);
            await _db.SaveChangesAsync();
        }

        public async Task ClearAsync()
        {
            var cart = await GetCartAsync();
            if (cart == null) return;

            _db.TbOrderDetails.RemoveRange(cart.TbOrderDetails);
            cart.Quanlity = 0;
            cart.TotalAmount = 0;
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetItemCountAsync()
        {
            var cart = await GetCartAsync();
            return cart?.TbOrderDetails.Sum(d => d.Quantity ?? 0) ?? 0;
        }

        public async Task MergeSessionCartToUserAsync(int accountId)
        {
            var sessionOrderId = Session.GetInt32(SessionCartOrderIdKey);
            if (!sessionOrderId.HasValue) return;

            var cartStatusId = await EnsureStatusAsync("Cart");

            var sessionCart = await _db.TbOrders
                .Include(o => o.TbOrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == sessionOrderId.Value && o.OrderStatusId == cartStatusId);

            if (sessionCart == null) return;

            var userCart = await _db.TbOrders
                .Include(o => o.TbOrderDetails)
                .FirstOrDefaultAsync(o => o.AccountId == accountId && o.OrderStatusId == cartStatusId);

            if (userCart == null)
            {
                sessionCart.AccountId = accountId;
                await RecomputeTotalsAsync(sessionCart);
                await _db.SaveChangesAsync();
            }
            else
            {
                foreach (var line in sessionCart.TbOrderDetails.ToList())
                {
                    var existing = userCart.TbOrderDetails.FirstOrDefault(d => d.ProductId == line.ProductId);
                    if (existing == null)
                    {
                        userCart.TbOrderDetails.Add(new TbOrderDetail
                        {
                            OrderId = userCart.OrderId,
                            ProductId = line.ProductId,
                            Price = line.Price,
                            Quantity = line.Quantity
                        });
                    }
                    else
                    {
                        existing.Quantity = (existing.Quantity ?? 0) + (line.Quantity ?? 0);
                    }
                }

                _db.TbOrderDetails.RemoveRange(sessionCart.TbOrderDetails);
                _db.TbOrders.Remove(sessionCart);

                await RecomputeTotalsAsync(userCart);
                await _db.SaveChangesAsync();
            }

            Session.Remove(SessionCartOrderIdKey);
        }

        public async Task<bool> CheckoutAsync(string customerName, string phone, string address)
        {
            var cart = await GetCartAsync();
            if (cart == null) return false;

            if (cart.TbOrderDetails.Count == 0) return false;

            await RecomputeTotalsAsync(cart);

            var pendingId = await EnsureStatusAsync("Pending");

            cart.CustomerName = customerName;
            cart.Phone = phone;
            cart.Address = address;
            cart.Code = GenerateOrderCode();
            cart.OrderStatusId = pendingId;
            cart.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // After checkout, remove session cart id
            if (!CurrentAccountId.HasValue)
            {
                Session.Remove(SessionCartOrderIdKey);
            }

            return true;
        }

        private async Task RecomputeTotalsAsync(TbOrder cart)
        {
            // Reload details from db to ensure tracked state is consistent
            await _db.Entry(cart).Collection(c => c.TbOrderDetails).LoadAsync();

            var totalQty = cart.TbOrderDetails.Sum(d => d.Quantity ?? 0);
            var totalAmount = cart.TbOrderDetails.Sum(d => Convert.ToInt32((d.Price ?? 0) * (d.Quantity ?? 0)));

            cart.Quanlity = totalQty;
            cart.TotalAmount = totalAmount;
            cart.ModifiedDate = DateTime.UtcNow;
        }

        private async Task<int> EnsureStatusAsync(string name)
        {
            var status = await _db.TbOrderStatuses.FirstOrDefaultAsync(s => s.Name == name);
            if (status != null) return status.OrderStatusId;

            status = new TbOrderStatus { Name = name, Description = name };
            _db.TbOrderStatuses.Add(status);
            await _db.SaveChangesAsync();
            return status.OrderStatusId;
        }

        private static string GenerateOrderCode()
        {
            // 10 chars max due to schema (nchar(10))
            return DateTime.UtcNow.ToString("yyMMddHHmm");
        }
    }
}