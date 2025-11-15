using Harmic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Harmic.Controllers
{
    public class OrderController : Controller
    {
        private readonly HarmicContext _context;

        [ActivatorUtilitiesConstructor] 
        public OrderController(HarmicContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
