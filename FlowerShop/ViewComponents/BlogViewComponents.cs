using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.ViewComponents
{
    public class BlogViewComponent : ViewComponent
    {
        private readonly QlbhtContext _context;

        public BlogViewComponent(QlbhtContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy danh sách blog active, sắp xếp giảm dần theo BlogId
            var blogs = await _context.Blogs.Include(b => b.ProductCategory)
                .Where(b => b.IsActive == true) // fix nullable bool
                .OrderByDescending(b => b.BlogId)
                .ToListAsync();

            return View(blogs); // trả về IViewComponentResult
        }
    }
}
