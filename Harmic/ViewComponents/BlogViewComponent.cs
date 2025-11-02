using Harmic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harmic.ViewComponents
{
    public class BlogViewComponent : ViewComponent
    {
        private readonly HarmicContext _context;

        public BlogViewComponent(HarmicContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var item = _context.TbBlogs.Where(m => (bool)m.IsActive);

            return await Task.FromResult<IViewComponentResult>(View(item.OrderByDescending(m => m.BlogId).ToList()));
        }
    }
}
