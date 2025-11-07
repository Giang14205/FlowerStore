using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult DetailCheckout()
        {
            return View();
        }
    }
}
