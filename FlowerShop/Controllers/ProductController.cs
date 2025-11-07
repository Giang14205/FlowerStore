using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult DetailProduct()
        {
            return View();
        }
    }
}
