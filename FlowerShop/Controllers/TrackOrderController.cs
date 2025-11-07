using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class TrackOrderController : Controller
    {
        public IActionResult DetailTrack()
        {
            return View();
        }
    }
}
