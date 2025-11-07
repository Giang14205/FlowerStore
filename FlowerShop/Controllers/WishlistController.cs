using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class WishlistController : Controller
    {
        public IActionResult DetailWishlist()
        {
            return View();
        }
    }
}
