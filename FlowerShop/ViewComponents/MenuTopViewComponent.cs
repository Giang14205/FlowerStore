using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.ViewComponents
{
    public class MenuTopViewComponent : ViewComponent
    {
        private readonly QlbhtContext _context;
        public MenuTopViewComponent(QlbhtContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = _context.Menus.Where(i =>(bool)i.IsActive).
                OrderBy(i => i.SortOrder).ToList();
            return await Task.FromResult<IViewComponentResult>(View(items));
        }

    }
}
