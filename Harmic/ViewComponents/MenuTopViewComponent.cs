using Harmic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harmic.ViewComponents
{
    public class MenuTopViewComponent : ViewComponent
    {
        private readonly HarmicContext _context;

        public MenuTopViewComponent(HarmicContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool admin = false)
        {
            var menus = await _context.TbMenus
                .AsNoTracking()
                .Where(m => admin || m.IsActive)
                .OrderBy(m => m.Position ?? int.MaxValue)
                .ToListAsync();

            return View(menus);
        }
    }
}
