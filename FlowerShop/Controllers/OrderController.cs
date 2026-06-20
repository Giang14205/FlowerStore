using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowerShop.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FlowerShop.Controllers
{
    // Bắt buộc người dùng phải đăng nhập mới truy cập được vào các Action trong Controller này
    [Authorize]
    public class OrderController : Controller
    {
        private readonly QlbhtContext _context;

        public OrderController(QlbhtContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH ĐƠN HÀNG CỦA RIÊNG TÀI KHOẢN ĐANG ĐĂNG NHẬP
        // Đường dẫn: /Order/Tracking
        public async Task<IActionResult> Tracking()
        {
            // Lấy ID người dùng từ Cookie định danh (ClaimTypes.NameIdentifier) do LoginController thiết lập
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                // Nếu không lấy được ID (phiên làm việc hết hạn), yêu cầu quay lại trang đăng nhập
                return RedirectToAction("DetailLogin", "Login");
            }

            // LOGIC CHUẨN: Lọc chính xác các đơn hàng có UserId trùng với người đang đăng nhập
            var orders = await _context.Orders
                .Include(o => o.OrderStatus)
                .Where(o => o.UserId == userId) // Đã khớp hoàn toàn với thuộc tính UserId trong Model của bạn
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        // HỦY ĐƠN 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId) // Phải khớp với tên trong form
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("DetailLogin", "Login");
            }

            // Tìm đơn hàng của chính user này
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái cho phép hủy (ví dụ <= 3)
            if (order.OrderStatusId > 3)
            {
                TempData["ErrorMessage"] = "Đơn hàng này không thể hủy vì đã được xác nhận hoặc đang vận chuyển.";
                return RedirectToAction("Tracking");
            }

            // Cập nhật trạng thái 
            order.OrderStatusId = 6;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đơn hàng #" + orderId + " đã được hủy thành công.";
            return RedirectToAction("Tracking");
        }


        // 2. TRANG XEM CHI TIẾT MỘT ĐƠN HÀNG (CÓ BẢO MẬT CHỐNG XEM TRỘM)
        // Đường dẫn: /Order/DetailOrder?id=1
        public async Task<IActionResult> DetailOrder(int id)
        {
            // Lấy ID người dùng đang đăng nhập hiện tại
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("DetailLogin", "Login");
            }

            // LOGIC BẢO MẬT: Tìm đơn hàng có mã trùng khớp VÀ phải thuộc sở hữu của chính người này
            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.PayMethod)
                .Include(o => o.OrderItems)
                    .ThenInclude(item => item.Product) // Kết nối bảng Product lấy thông tin ImagePopular và ProductName
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                // Nếu đơn hàng không tồn tại hoặc ông A cố tình đổi ID trên URL để xem đơn của ông B ➔ Báo lỗi không tìm thấy luôn
                return NotFound();
            }

            return View(order);
        }
    }
}