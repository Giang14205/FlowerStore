using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly QlbhtContext _context;
        public ProductController(QlbhtContext context)
        {
            _context = context;
        }
        [Route("/product/{alias}-{id}.html")]
        public async Task<IActionResult> DetailProduct(int? id)
        {
            if( id == null|| _context.Products==null)
            {
                return NotFound();
              
            }
            var product = await _context.Products.Include(i => i.ProductCategory).FirstOrDefaultAsync(m => m.ProductId == id);
                if(product ==null )
            {
                return NotFound();
            }
            ViewBag.feedback = _context.FeedbackCustomers.Where(i => i.ProductId == id && i.IsActive==true).ToList();
            ViewBag.productrelate = _context.Products.Where(i => i.ProductId != id && i.ProductCategoryId == product.ProductCategoryId).Where(i=>i.IsNew==true).Where(i=>i.IsActive==true).OrderByDescending(i => i.ProductId).ToList();
            ViewBag.imageproduct = _context.ProductImages.Where(i => i.ProductId == id).Where(i => i.IsDefault == true).ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>AddFeedback(FeedbackCustomer feedback)
        {
            var product = await _context.Products.FindAsync(feedback.ProductId);

            // Nếu ID bị sai hoặc ko tìm thấy sản phẩm
            if (product == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                feedback.IsActive = true;
              
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction("DetailProduct", new { id = product.ProductId, alias = product.Alias });

            }
            return RedirectToAction("DetailProduct", new { id = product.ProductId, alias = product.Alias });
        }
    }
}
