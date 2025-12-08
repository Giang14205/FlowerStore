using FlowerShop.Models;
using FlowerShop.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly QlbhtContext _context;
        public LoginController(QlbhtContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(User account)
        {
            if (account == null)
            {
                return NotFound();
            }
            string password = HashMD5.GetMD5(account.Password);
            var check = _context.Users.Where(m => m.UserName == account.UserName && m.Password == password).FirstOrDefault();
            if (check == null)
            {
                Function._Message = "Invalid Username or Password";
                return RedirectToAction("Index", "Login");


            }
            Function._Message = string.Empty;
            Function._AccountId = check.UserId;
            Function._Username = check.UserName;
            return RedirectToAction("Index", "Home");

        }
        
    }
}
