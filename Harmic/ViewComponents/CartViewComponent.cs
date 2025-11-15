using Microsoft.AspNetCore.Mvc;
using Harmic.Models;
using System.Linq;

namespace Harmic.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly HarmicContext _ctx;
        public CartViewComponent(HarmicContext ctx) => _ctx = ctx;

        public IViewComponentResult Invoke()
        {
            var orders = _ctx.TbOrders.ToList();
            var products = _ctx.TbProducts.ToList();
            return View((orders, products)); 
        }
    }
}