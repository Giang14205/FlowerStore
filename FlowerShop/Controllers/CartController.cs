using FlowerShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlowerShop.Controllers
{
    public class CartController : Controller
    {
        private readonly QlbhtContext _context;

        public CartController(QlbhtContext context)
        {
            _context = context;
        }

        // --- ĐỂ CÁC CLASS PAYLOAD ĐỒNG BỘ CHỮ VIẾT THƯỜNG VỚI JSON TỪ FRONTEND ---
        public class CartItemInput
        {
            public int productId { get; set; }
            public int quantity { get; set; }
        }

        public class CustomBouquetPayload
        {
            public string wrapName { get; set; }
            public string customImage { get; set; } // Nhận chuỗi Base64 ảnh tổng thể chụp từ studio trơn sạch
            public List<CartItemInput> items { get; set; } = new List<CartItemInput>();
        }

        [HttpGet]
        public async Task<IActionResult> TuPhoiHoa()
        {
            var danhSachNguyenLieu = await _context.Products
                .Where(p => p.IsMaterial == true) // Đảm bảo chỉ lấy nguyên liệu hoa lẻ, nơ, giấy
                .ToListAsync();
            return View(danhSachNguyenLieu);
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomBouquet([FromBody] CustomBouquetPayload payload)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập trước khi thêm hoa vào giỏ hàng nhé!" });
            }

            if (payload == null || payload.items == null || payload.items.Count == 0)
            {
                return Json(new { success = false, message = "Bó hoa của bạn chưa có nguyên liệu nào!" });
            }

            try
            {
                // 1. Lấy mã User đang đăng nhập hệ thống
                var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(claimUserId))
                {
                    return Json(new { success = false, message = "Không tìm thấy mã người dùng!" });
                }
                int userId = int.Parse(claimUserId);

                // 2. Tìm giỏ hàng (Cart) của User, nếu chưa có thì tạo mới luôn
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CreatedDate = DateTime.Now };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync(); // Lưu để sinh ra CartId tự tăng
                }

               
                // Trói chặt toàn bộ mớ hoa lẻ vào chung một dấu ấn vết tích Note phân tách bằng dấu gạch đứng |
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss"); // Tạo 1 mã thời gian duy nhất cho CẢ BÓ
                string base64Data = !string.IsNullOrEmpty(payload.customImage) ? payload.customImage : "";

                // Chuỗi bouquetMarker này sẽ giống nhau Y HỆT cho mọi nguyên liệu lẻ trong bó hoa này
                string bouquetMarker = $"Bó tự phối [{timestamp}] - Gói: {payload.wrapName}|{base64Data}";

                // 4. Lưu từng dòng nguyên liệu lẻ vào bảng CartItem với chung một dấu ấn Note
                foreach (var item in payload.items)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == item.productId);
                    if (product == null) continue;

                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = item.productId,
                        Quantity = item.quantity,
                        Price = product.ProductPrice,
                        Note = bouquetMarker          // 🔥 TẤT CẢ HOA LẺ SẼ MANG CHUNG 1 CHUỖI NOTE GIỐNG NHAU TUYỆT ĐỐI
                    };
                    _context.CartItems.Add(cartItem);
                }

                // Thực hiện lưu thay đổi xuống SQL Server
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            // 🔥 BẮT LỖI RIÊNG CHO DATABASE: Để nếu cột Note bị quá độ dài nvarchar nó sẽ báo thẳng cho má biết
            catch (DbUpdateException dbEx)
            {
                string sqlError = dbEx.InnerException != null ? dbEx.InnerException.Message : dbEx.Message;
                System.Diagnostics.Debug.WriteLine("🚨 LỖI SQL SERVER: " + sqlError);
                return Json(new { success = false, message = "Lỗi Database từ chối lưu: " + sqlError });
            }
            catch (Exception ex)
            {
                string sysError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi hệ thống Controller: " + sysError });
            }
        }
    }
}