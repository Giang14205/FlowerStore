using Microsoft.AspNetCore.Mvc;
using FlowerShop.Utilities;
namespace FlowerShop.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        [Area("Admin")]
        public IActionResult Index()
        {
            if (!Function.IsLogin())
                return RedirectToAction("Index", "Login");
            return View();
        }
    }
}
