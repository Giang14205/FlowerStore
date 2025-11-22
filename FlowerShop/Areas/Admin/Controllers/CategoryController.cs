using FlowerShop.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace FlowerShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
      
        private readonly QlbhtContext _context;
        public CategoryController(QlbhtContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var items = _context.ProductCategories;
            return View(items);
        }
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken ]
        public ActionResult Add(ProductCategory model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;

                _context.ProductCategories.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
          
        }
        
    }
}
