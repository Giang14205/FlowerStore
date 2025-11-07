using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult DetailContact()
        {
            return View();
        }
    }
}
