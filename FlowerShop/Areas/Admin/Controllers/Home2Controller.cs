using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Areas.Admin.Controllers
{
    public class Home2Controller : Controller
    {
        [Area("Admin")]

        public IActionResult Home2Index()
        {
            return View();
        }
    }
}
