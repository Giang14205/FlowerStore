using FlowerShop.Models;
using FlowerShop.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegisterController : Controller
    {
       
         private readonly QlbhtContext _context;
            public RegisterController(QlbhtContext context)
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
                var check = _context.Users.Where(m => m.UserName == account.UserName).FirstOrDefault();
                if (check != null)
                {
                    Function._Message = "Trùng tài khoản";
                    return RedirectToAction("Index", "Register");


                }
                Function._Message = string.Empty;
                account.Password = HashMD5.GetMD5(account.Password != null ? account.Password : "");
                _context.Add(account);
                _context.SaveChanges();
                return RedirectToAction("Index", "Login");

            }
        
        
    }
}
