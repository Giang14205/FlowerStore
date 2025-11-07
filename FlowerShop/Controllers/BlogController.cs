using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult DetailBlog()
        {
            return View();
        }
    }
}
