using FlowerShop.Models;
using FlowerShop.Services.Vnpay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace FlowerShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly QlbhtContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly List<string> _vinhAreas = new List<string> {
    "Phường Hưng Bình,Thành phố Vinh", "Phường Lê Lợi,Thành phố Vinh", "Phường Quán Bàu,Thành phố Vinh", "Phường Hà Huy Tập,Thành phố Vinh","Phường Trường Thi,Thành phố Vinh","Phường Bến Thủy,Thành phố Vinh"
};
        public CheckoutController(QlbhtContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // ĐỊNH NGHĨA LỚP ĐỆM CHUNG ĐỂ GỘP DỮ LIỆU THANH TOÁN
        public class CheckoutItemDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public string Note { get; set; }
            public string ProductImage { get; set; }
        }

        // HÀM TRỢ GIÚP: GỘP GIỎ HÀNG TỪ SESSION VÀ DATABASE
        private List<CheckoutItemDto> GetUnifiedCart(int userId)
        {
            var unifiedCart = new List<CheckoutItemDto>();

            // 1. Lấy sản phẩm thường từ Session "GioHang"
            var sessionData = HttpContext.Session.GetString("GioHang");
            if (!string.IsNullOrEmpty(sessionData))
            {
                var sessionCart = JsonSerializer.Deserialize<List<CartItemVM>>(sessionData);
                if (sessionCart != null)
                {
                    foreach (var item in sessionCart)
                    {
                        unifiedCart.Add(new CheckoutItemDto
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Quantity = item.Quantity,
                            Price = (decimal)item.Price,
                            Note = "Sản phẩm cửa hàng",
                            ProductImage = item.Image
                        });
                    }
                }
            }

            // 2. Lấy sản phẩm phối từ Bảng CartItems dưới Database
            var dbCartItems = _context.CartItems
                .Include(x => x.Product)
                .Where(x => x.Cart.UserId == userId).ToList();

            foreach (var item in dbCartItems)
            {
                unifiedCart.Add(new CheckoutItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.ProductName ?? "Nguyên liệu lẻ",
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Note = item.Note
                });
            }

            return unifiedCart;
        }


        // 1. GET: Hiển thị giao diện lập hóa đơn thông tin đặt hàng
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Gọi hàm gộp giỏ hàng vạn năng
            var unifiedCart = GetUnifiedCart(userId);
            ViewBag.ShippingAreas = _vinhAreas;

            if (unifiedCart.Count == 0)
                return RedirectToAction("Index", "ShoppingCart");

            // Đếm số bó hoa tự phối dựa trên chuỗi Note độc bản dưới DB
            var customBouquetCount = unifiedCart
                .Where(x => !string.IsNullOrEmpty(x.Note) && x.Note.StartsWith("Bó tự phối"))
                .Select(x => x.Note)
                .Distinct()
                .Count();

            // Tổng tiền = Toàn bộ sản phẩm + (Số bó tự phối * 50.000đ công cắm)
            decimal totalItemsPrice = unifiedCart.Sum(x => x.Quantity * x.Price);
            decimal finalTotal = totalItemsPrice + (customBouquetCount * 50000);

            ViewBag.CartItems = unifiedCart; // Truyền danh sách đã gộp ra ngoài View hiển thị
            ViewBag.Total = finalTotal;

            return View();
        }

        [Authorize]
        [HttpPost]
        // CHỈ CẦN THÊM DateTime NgayGiao VÀO ĐÂY LÀ ĐỦ
        public IActionResult Checkout(string HoTen, string DienThoai, string DiaChi, string GhiChu, string PaymentMethod, DateTime NgayGiao)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Gọi hàm gộp giỏ hàng vạn năng
            var unifiedCart = GetUnifiedCart(userId);
            if (!_vinhAreas.Contains(DiaChi))
            {
                ModelState.AddModelError("DiaChi", "Cửa hàng chỉ hỗ trợ giao hàng trong TP. Vinh.");
                ViewBag.ShippingAreas = _vinhAreas;
                return View();
            }

            if (unifiedCart.Count == 0)
                return RedirectToAction("Index", "ShoppingCart");

            // Tính toán tiền
            var customBouquetCount = unifiedCart
                .Where(x => !string.IsNullOrEmpty(x.Note) && x.Note.StartsWith("Bó tự phối"))
                .Select(x => x.Note)
                .Distinct()
                .Count();
            decimal finalOrderTotal = unifiedCart.Sum(x => x.Quantity * x.Price) + (customBouquetCount * 50000);

            string orderNumber = $"DH{DateTime.Now:yyyyMMddHHmmss}-{userId}";

            // XỬ LÝ AN TOÀN: Nếu khách không chọn ngày, mặc định là ngày mai
            if (NgayGiao == DateTime.MinValue) NgayGiao = DateTime.Now.AddDays(1);

            string deliveryDateStr = NgayGiao.ToString("dd/MM/yyyy");
            string finalNote = $"[Ngày giao dự kiến: {deliveryDateStr}] - {GhiChu}";

            // A. Lưu thông tin
            var order = new Order
            {
                OrderDate = DateTime.Now,
                UserId = userId,
                OrderNumber = orderNumber,
                FullName = HoTen,
                Phone = DienThoai,
                Address = DiaChi,
                Note = finalNote,
                TotalAmount = finalOrderTotal,
                PayMethodId = (PaymentMethod == "VNPAY") ? 2 : 1,
                OrderStatusId = 1
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // B. Lưu chi tiết đơn hàng (Giữ nguyên)
            foreach (var item in unifiedCart)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    LineTotal = item.Quantity * item.Price,
                    Note = item.Note
                });
            }
            _context.SaveChanges();

            // C. Dọn dẹp giỏ hàng (Giữ nguyên)
            HttpContext.Session.Remove("GioHang");
            var dbCartItems = _context.CartItems.Where(x => x.Cart.UserId == userId).ToList();
            if (dbCartItems.Count > 0)
            {
                _context.CartItems.RemoveRange(dbCartItems);
                _context.SaveChanges();
            }

            // D. Điều hướng (Giữ nguyên)
            if (PaymentMethod == "VNPAY")
            {
                var paymentModel = new FlowerShop.Models.Vnpay.PaymentInformationModel
                {
                    OrderId = order.OrderId.ToString(),
                    Amount = (double)order.TotalAmount,
                    OrderDescription = $"Thanh toan don hang hoa: {order.OrderNumber}",
                    OrderType = "other",
                    Name = HoTen
                };
                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
                return Redirect(paymentUrl);
            }

            return RedirectToAction("DetailSuccess", new { id = order.OrderId });
        }

        // 3. GET: Hiển thị giao diện thông báo đặt hàng thành công
        public IActionResult DetailSuccess(int id)
        {
            var order = _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefault(x => x.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }
    }
}