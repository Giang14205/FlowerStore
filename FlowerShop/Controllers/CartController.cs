using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class CartController : Controller
    {
        public IActionResult DetailCart()
        {
            return View();
        }
    }
}
