using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Harmic.Models;
using Microsoft.EntityFrameworkCore;

namespace Harmic.Services
{
    public class ChatRetrievalService
    {
        private readonly HarmicContext _db;

        public ChatRetrievalService(HarmicContext db)
        {
            _db = db;
        }

        // Build a small context string from SQL Server for RAG
        public async Task<string> BuildContextAsync(string question, int limit = 5, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(question))
                return string.Empty;

            var q = question.ToLowerInvariant();
            var sb = new StringBuilder();

            // Order lookup by code if user mentions order
            if (q.Contains("đơn hàng") || q.Contains("order") || q.Contains("mã đơn"))
            {
                var code = ExtractOrderCode(question);
                if (!string.IsNullOrWhiteSpace(code))
                {
                    var orders = await _db.TbOrders.AsNoTracking()
                        .Where(o => o.Code != null && o.Code.Contains(code))
                        .OrderByDescending(o => o.CreatedDate)
                        .Take(limit)
                        .Select(o => new
                        {
                            o.Code,
                            o.CustomerName,
                            o.Phone,
                            o.Address,
                            o.TotalAmount,
                            o.CreatedDate,
                            Status = o.OrderStatus != null ? o.OrderStatus.Name : ""
                        })
                        .ToListAsync(ct);

                    if (orders.Count > 0)
                    {
                        sb.AppendLine("FACTS: Đơn hàng gần khớp:");
                        foreach (var o in orders)
                            sb.AppendLine($"- Code: {o.Code}, Khách: {o.CustomerName}, Trạng thái: {o.Status}, Tổng tiền: {o.TotalAmount}, Ngày: {o.CreatedDate}");
                        sb.AppendLine();
                    }
                }
            }

            // Product lookup
            if (q.Contains("sản phẩm") || q.Contains("product") || q.Contains("giá") || q.Contains("mua"))
            {
                var products = await _db.TbProducts.AsNoTracking()
                    .OrderByDescending(p => p.ModifiedDate ?? p.CreatedDate)
                    .Where(p =>
                        (p.Title != null && EF.Functions.Like(p.Title, $"%{question}%")) ||
                        (p.Description != null && EF.Functions.Like(p.Description, $"%{question}%")))
                    .Take(limit)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Title,
                        p.Alias,
                        p.Price,
                        p.Image,
                        Category = p.CategoryProduct != null ? p.CategoryProduct.Title : ""
                    })
                    .ToListAsync(ct);

                if (products.Count > 0)
                {
                    sb.AppendLine("FACTS: Sản phẩm liên quan:");
                    foreach (var p in products)
                        sb.AppendLine($"- {p.Title} (Danh mục: {p.Category}) | Giá: {p.Price}");
                    sb.AppendLine();
                }
            }

            // Category lookup
            if (q.Contains("danh mục") || q.Contains("category") || q.Contains("loại"))
            {
                var cats = await _db.TbProductCategories.AsNoTracking()
                    .OrderByDescending(c => c.ModifiedDate ?? c.CreatedDate)
                    .Where(c =>
                        (c.Title != null && EF.Functions.Like(c.Title, $"%{question}%")) ||
                        (c.Description != null && EF.Functions.Like(c.Description, $"%{question}%")))
                    .Take(limit)
                    .Select(c => new { c.Title, c.Description })
                    .ToListAsync(ct);

                if (cats.Count > 0)
                {
                    sb.AppendLine("FACTS: Danh mục liên quan:");
                    foreach (var c in cats)
                        sb.AppendLine($"- {c.Title}: {c.Description}");
                    sb.AppendLine();
                }
            }

            // Blog/News (optional)
            if (q.Contains("blog") || q.Contains("tin tức") || q.Contains("news"))
            {
                var news = await _db.TbNews.AsNoTracking()
                    .OrderByDescending(n => n.ModifiedDate ?? n.CreatedDate)
                    .Where(n =>
                        (n.Title != null && EF.Functions.Like(n.Title, $"%{question}%")) ||
                        (n.Description != null && EF.Functions.Like(n.Description, $"%{question}%")))
                    .Take(limit)
                    .Select(n => new { n.Title, n.Description })
                    .ToListAsync(ct);

                if (news.Count > 0)
                {
                    sb.AppendLine("FACTS: Bài viết/Tin tức liên quan:");
                    foreach (var n in news)
                        sb.AppendLine($"- {n.Title}: {n.Description}");
                    sb.AppendLine();
                }
            }

            // Fallback: if nothing matched, return a tiny catalog snapshot to help the model answer
            if (sb.Length == 0)
            {
                var topProducts = await _db.TbProducts.AsNoTracking()
                    .OrderByDescending(p => p.IsBestSeller)
                    .ThenByDescending(p => p.IsNew)
                    .ThenByDescending(p => p.ModifiedDate ?? p.CreatedDate)
                    .Take(limit)
                    .Select(p => new
                    {
                        p.Title,
                        p.Price,
                        Category = p.CategoryProduct != null ? p.CategoryProduct.Title : ""
                    })
                    .ToListAsync(ct);

                if (topProducts.Count > 0)
                {
                    sb.AppendLine("FACTS: Một số sản phẩm tiêu biểu:");
                    foreach (var p in topProducts)
                        sb.AppendLine($"- {p.Title} (Danh mục: {p.Category}) | Giá: {p.Price}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static string ExtractOrderCode(string text)
        {
            // common pattern: 4-12 alphanumerics (tweak to your real codes)
            var m = Regex.Match(text, @"[A-Za-z0-9]{4,12}");
            return m.Success ? m.Value : string.Empty;
        }
    }
}