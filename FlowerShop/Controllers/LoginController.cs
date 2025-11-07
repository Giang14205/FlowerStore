using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult DetailLogin()
        {
            return View();
        }
    }
}
