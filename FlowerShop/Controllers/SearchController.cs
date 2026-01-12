using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FlowerShop.Controllers
{
    public class SearchController : Controller
    {
        private readonly QlbhtContext _context;
        public SearchController(QlbhtContext context)
        {
            _context = context;
        }
        [HttpGet]
        public  async Task<IActionResult> Index( string q)
        {
            if (string.IsNullOrEmpty(q))    
            {
                return RedirectToAction("DetailHome1", "Home1");
                
            }
            var results = await _context.Products.Where(p => p.ProductName.Contains(q) || p.ProductDescription.Contains(q)).ToListAsync();
            ViewBag.Keyword = q;
            return View(results);
        }
    }
}
