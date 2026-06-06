using FlowerShop.Models;
using FlowerShop.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims; // Cần thiết cho Claims
using Microsoft.AspNetCore.Authentication; // Cần thiết cho SignIn/SignOut
using Microsoft.AspNetCore.Authentication.Cookies; // Cần thiết cho Scheme Cookies
// Nếu chưa có, bạn cần thêm using Microsoft.EntityFrameworkCore; ở đây nếu muốn dùng Include

namespace FlowerShop.Controllers
{
    public class LoginController : Controller
    {
        private readonly QlbhtContext _context;
        public LoginController(QlbhtContext context)
        {
            _context = context;
        }

        // 1. DETAIL LOGIN (GET) - NHẬN VÀ LƯU returnUrl
        public IActionResult DetailLogin(string returnUrl) // Thêm tham số returnUrl
        {
            // Lưu lại returnUrl để truyền sang View
            ViewBag.ReturnUrl = returnUrl;
            return View("DetailLogin");
        }

        // 2. ĐĂNG NHẬP (POST) - SỬ DỤNG AUTHENTICATION
        [HttpPost]
        public async Task<IActionResult> Login(User user, string returnUrl) // Đổi sang async Task và thêm returnUrl
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                ViewBag.LoginError = "Vui lòng nhập đầy đủ email và mật khẩu!";
                ViewBag.ReturnUrl = returnUrl;
                return View("DetailLogin");
            }

            string password = HashMD5.GetMD5(user.Password.Trim());
            string email = user.Email.Trim().ToLower();

            var check = _context.Users
                .FirstOrDefault(m => m.Email.ToLower() == email && m.Password == password);
           
            if (check == null)
            {
                ViewBag.LoginError = "Email hoặc mật khẩu không đúng!";
                ViewBag.ReturnUrl = returnUrl;
                return View("DetailLogin");
            }


            var roleName = (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.RoleId
                            where ur.UserId == check.UserId
                            select r.RoleName).FirstOrDefault() ?? "Customer";



            //var a = new List<Claim>
            // {
            // new Claim(ClaimTypes.NameIdentifier, check.UserId.ToString()),
            // new Claim(ClaimTypes.Name, check.FullName ?? check.Email),
            // new Claim(ClaimTypes.Role, roleName) // <--- QUAN TRỌNG: Lưu quyền ở đây
            // };

            //var claimsIdentityy = new ClaimsIdentity(a, CookieAuthenticationDefaults.AuthenticationScheme);

            //await HttpContext.SignInAsync(
            //    CookieAuthenticationDefaults.AuthenticationScheme,
            //    new ClaimsPrincipal(claimsIdentityy));
            // 🛑 BƯỚC SỬA 1: TẠO CLAIMS VÀ SIGN IN (Authentication)
            var claims = new List<Claim>
            {
                // QUAN TRỌNG: Lấy UserId để ShoppingCartController sử dụng
                new Claim(ClaimTypes.NameIdentifier, check.UserId.ToString()),
                new Claim(ClaimTypes.Name, check.FullName ?? check.Email),
                 new Claim(ClaimTypes.Role, roleName) // <--- QUAN TRỌNG: Lưu quyền ở đây
            };

            // TẠM THỜI BỎ QUA LOGIC LẤY ROLE ĐỂ TRÁNH PHỨC TẠP

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Ghi nhận đăng nhập vào hệ thống (Set Cookie)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Cập nhật LastLogin (giữ nguyên logic của bạn)
            check.LastLogin = DateTime.Now;
            _context.Update(check);
            _context.SaveChanges();

            // 🛑 BƯỚC SỬA 2: CHUYỂN HƯỚNG SỬ DỤNG returnUrl để thoát vòng lặp
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl); // Chuyển thẳng về trang Checkout
            }

            if (roleName == "Admin")
            {
                // Chuyển hướng đến Area Admin, Controller Home, Action Index
                return RedirectToAction("Index", "Login", new { area = "Admin" });
            }


            return RedirectToAction("DetailHome1", "Home1");
        }

        // 3. ĐĂNG KÝ (POST) - Sau khi tạo tài khoản, tự động đăng nhập người dùng
        [HttpPost]
        public async Task<IActionResult> Register(User user, string returnUrl) // Thêm returnUrl
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                ViewBag.RegisterError = "Không được để trống email hoặc mật khẩu!";
                ViewBag.ReturnUrl = returnUrl;
                return View("DetailLogin");
            }

            string email = user.Email.Trim().ToLower();
            var check = _context.Users.FirstOrDefault(m => m.Email.ToLower() == email);
            if (check != null)
            {
                ViewBag.RegisterError = "Email đã tồn tại!";
                ViewBag.ReturnUrl = returnUrl;
                return View("DetailLogin");
            }

            // ... (Logic Hash Password, gán Status, LastLogin giữ nguyên) ...
            user.Email = email;
            user.Password = HashMD5.GetMD5(user.Password.Trim());
            user.Status = 1;
            user.LastLogin = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            // ... (Logic UserRole giữ nguyên) ...
            var newUserRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = 2
            };

            _context.UserRoles.Add(newUserRole);
            _context.SaveChanges();

            // 🛑 BƯỚC SỬA 3: TỰ ĐỘNG SIGN IN SAU KHI ĐĂNG KÝ THÀNH CÔNG 🛑
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // 🛑 BƯỚC SỬA 4: CHUYỂN HƯỚNG TỪ ĐĂNG KÝ 🛑
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl); // Chuyển thẳng về trang Checkout
            }

            // Nếu không có returnUrl, chuyển về trang chủ
            return RedirectToAction("DetailHome1", "Home1");
        }
    }
}