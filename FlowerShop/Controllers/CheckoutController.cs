using FlowerShop.Models;
using FlowerShop.Services.Vnpay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace FlowerShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly QlbhtContext _context;
        private readonly IVnPayService _vnPayService;

        // 1. Tiêm (Inject) Database và VNPay vào Controller
        public CheckoutController(QlbhtContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // 2. Hàm hiển thị Form điền thông tin (thay cho DetailCheckout của bạn)
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "ShoppingCart");

            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(X => X.Price * X.Quantity);
            return View();
        }

        // 3. Hàm xử lý lưu Database khi bấm nút "Đặt hàng"
        [Authorize]
        [HttpPost]
        public IActionResult Checkout(string HoTen, string DienThoai, string DiaChi, string GhiChu, string PaymentMethod)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "ShoppingCart");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdString);
            decimal cartTotal = (decimal)cart.Sum(x => x.Price * x.Quantity);

            string orderNumber = $"DH{DateTime.Now:yyyyMMddHHmmss}-{userId}";

            // Tạo đối tượng Đơn hàng
            var order = new Order
            {
                OrderDate = DateTime.Now,
                UserId = userId,
                OrderNumber = orderNumber,
                FullName = HoTen,
                Phone = DienThoai,
                Address = DiaChi,
                Note = GhiChu,
                //ShippingAddress = $"Người nhận: {HoTen} | SĐT: {DienThoai} | Địa chỉ: {DiaChi}. Ghi chú: {GhiChu}",
                TotalAmount = cartTotal,
                PayMethodId = (PaymentMethod == "COD") ? 1 : 2,
                OrderStatusId = 1 // Trạng thái: Mới đặt
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Tạo chi tiết Đơn hàng
            foreach (var item in cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = (decimal)item.Price,
                    LineTotal = (decimal)(item.Price * item.Quantity)
                };
                _context.OrderItems.Add(orderItem);
            }
            _context.SaveChanges();

            // Xử lý luồng VNPay
            if (PaymentMethod == "VNPAY")
            {
                var paymentModel = new FlowerShop.Models.Vnpay.PaymentInformationModel
                {
                    OrderId = order.OrderId.ToString(),
                    Amount = (double)order.TotalAmount,
                    OrderDescription = $"Thanh toán đơn hàng: {order.OrderNumber}",
                    OrderType = "other",
                    Name = HoTen
                };
                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);

                SaveCart(new List<CartItemVM>()); // Xóa giỏ hàng tạm
                return Redirect(paymentUrl);
            }

            // Xử lý luồng COD
            SaveCart(new List<CartItemVM>());
            return RedirectToAction("DetailSuccess", new { id = order.OrderId });
        }

        // 4. Hàm hiển thị hóa đơn thành công
        public IActionResult DetailSuccess(int id)
        {
            var order = _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefault(x => x.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // --- CÁC HÀM HỖ TRỢ XỬ LÝ SESSION GIỎ HÀNG ---
        private List<CartItemVM> GetCart()
        {
            var session = HttpContext.Session.GetString("GioHang");
            if (session != null)
            {
                return JsonSerializer.Deserialize<List<CartItemVM>>(session);
            }
            return new List<CartItemVM>();
        }

        private void SaveCart(List<CartItemVM> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("GioHang", json);
        }
    }
}