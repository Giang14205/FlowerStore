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
            var check = _context.Users.Where(m => m.Email == account.Email && m.Password == password).FirstOrDefault();
            if (check == null)
            {
                Function._Message = "Email hoặc mật khẩu của bạn không đúng";
                return RedirectToAction("Index", "Login");


            }
            if (check.Status == 0)
            {
                // Nếu trạng thái bằng 0, chặn đứng không cho nạp Session đăng nhập
                Function._Message = "Tài khoản của bạn đã bị khóa hoặc vô hiệu hóa!";
                return RedirectToAction("Index", "Login");
            }
            Function._Message = string.Empty;
            Function._AccountId = check.UserId;
            Function._Username = check.UserName;
            return RedirectToAction("Index", "Home");

        }
        
    }
}
