using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FlowerShop.Controllers
{
    public class Home1Controller : Controller
    {
        private readonly QlbhtContext _context;
        public Home1Controller(QlbhtContext context)
        {
            _context = context;
        }
        public IActionResult DetailHome1(int? id)
        {
            var productNew = _context.Products
                .Where(p => p.IsNew == true && p.IsActive == true)
                .OrderByDescending(p => p.ProductId)
                .ToList();

            // Lấy ID của tất cả sản phẩm mới
            var productIds = productNew.Select(p => p.ProductId).ToList();

            // Lấy tất cả ảnh của các sản phẩm mới
            var productImages = _context.ProductImages
                .Where(i => productIds.Contains(i.ProductId))
                .ToList();

            // Truyền dữ liệu qua ViewBag
            ViewBag.productCategories = _context.ProductCategories.ToList();
            ViewBag.productNew = productNew;
            ViewBag.imageproduct = productImages;
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
