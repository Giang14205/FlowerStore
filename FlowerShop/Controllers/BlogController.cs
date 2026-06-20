using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Controllers
{
    public class BlogController : Controller
    {
        private readonly QlbhtContext _context;
        public BlogController(QlbhtContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> DetailBlog(string alias , int id)
        {
            var blogs = await _context.Blogs
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.BlogId)
                .Take(3)
                .ToListAsync();
            ViewBag.blogrecent = _context.Blogs.Where(i => i.BlogId != id ).Where(i=>i.IsActive==true).OrderByDescending(i => i.BlogId).ToList();
            return View(blogs); // trả về IEnumerable<Blog>
        }
        [Route("/blog/{alias}-{id}.html")]
      
        public async Task<IActionResult> Details(string alias, int id)
        {
            var blog = await _context.Blogs

                .FirstOrDefaultAsync(b => b.BlogId == id && b.IsActive == true);

            if (blog == null)
                return NotFound();

            ViewBag.blogComment = _context.FeedbackCustomers
                .Where(c => c.BlogId == id)
                .ToList();
         
            return View(blog);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> AddComment(FeedbackCustomer feedback)
        {
            var blog = await _context.Blogs.FindAsync(feedback.BlogId);

            // Nếu ID bị sai hoặc ko tìm thấy sản phẩm
            if (blog == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                feedback.IsActive = true;

                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = blog.BlogId, alias = blog.Alias });

            }
            return RedirectToAction("Details", new { id = blog.BlogId, alias = blog.Alias });
        }
    }
}
