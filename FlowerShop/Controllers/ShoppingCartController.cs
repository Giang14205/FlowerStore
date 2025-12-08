using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Text.Json;
using System.Security.Claims; // Cần thiết để lấy ID người dùng
using Microsoft.AspNetCore.Authorization;// <--- CẦN THÊM DÒNG NÀY
using Microsoft.EntityFrameworkCore;
using FlowerShop.Services.Vnpay;
namespace FlowerShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly QlbhtContext _context;
        public ShoppingCartController(QlbhtContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        //Thêm vào giỏ hàng
        public IActionResult AddToCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);
            if (item !=null)
            {
                item.Quantity++;
            }
            else
            {
                var product = _context.Products.Find(productId);
                if (product == null) return NotFound();
                decimal finalPrice;
                if (product.PriceSale > 0)
                {
                    finalPrice = product.DiscountPrice.GetValueOrDefault(product.ProductPrice);
                    // Dùng GetValueOrDefault() để xử lý trường hợp DiscountPrice là nullable
                }
                else
                {
                    finalPrice = product.ProductPrice;
                }
                cart.Add(new CartItemVM
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Image = product.ImagePopular,
                    Price = finalPrice,
                    Quantity = 1
                });
                  
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // xóa khỏi giở hàng
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);
            if(item !=null)   
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // trang thanh toán
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index");
            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(X => X.Price* X.Quantity);
            return View();
        }

        [Authorize]
        [HttpPost]
       
        public IActionResult Checkout(string HoTen, string DienThoai, string DiaChi ,string GhiChu, string PaymentMethod)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index");
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdString);
            decimal cartTotal = (decimal)cart.Sum(x => x.Price * x.Quantity); // <-- KHẮC PHỤC LỖI cartTotal

            string orderNumber = $"DH{DateTime.Now:yyyyMMddHHmmss}-{userId}";

            // 1. Tạo mới đối tượng Cart (Model gốc của bạn)
            var order = new Order
            {
                OrderDate = DateTime.Now,
                UserId = userId, // Dùng ID (int) đã lấy, không còn là nullable (int?)
                OrderNumber = orderNumber,
                ShippingAddress = $"Người nhận: {HoTen} | SĐT: {DienThoai} | Địa chỉ: {DiaChi}. Ghi chú: {GhiChu}",
                TotalAmount = cartTotal,
                PayMethodId = (PaymentMethod == "COD") ? 1 : 2,
                OrderStatusId = 1
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // 2. Tạo chi tiết đơn hàng (CartItem gốc của bạn)
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

            // 3. Xóa Session và báo thành công
            if (PaymentMethod == "VNPAY")
            {
                // 1. Tạo PaymentInformationModel từ dữ liệu đơn hàng
                var paymentModel = new FlowerShop.Models.Vnpay.PaymentInformationModel
                {
                    OrderId = order.OrderId.ToString(), // Mã đơn hàng dùng làm TxnRef (string)
                    Amount = (double)order.TotalAmount, // Tổng tiền (double)
                    OrderDescription = $"Thanh toán đơn hàng: {order.OrderNumber}",
                    OrderType = "other",
                    Name = HoTen // Tên người nhận
                };
                var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);

                // 3. Xóa giỏ hàng (vì đơn hàng đã được tạo trong DB)
                SaveCart(new List<CartItemVM>());

                // 4. Redirect thẳng đến URL VNPAY
                return Redirect(paymentUrl); // <<< Sửa thành Redirect(url)
            }
            SaveCart(new List<CartItemVM>());
            return RedirectToAction("DetailSuccess", new { id = order.OrderId });
        }
        public IActionResult DetailSuccess(int id)
        {
            var order = _context.Orders
        .Include(x => x.OrderItems)
        .ThenInclude(x => x.Product)
        .FirstOrDefault(x => x.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
            
        }
        // Action này trả về JSON để AJAX gọi
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                // Cập nhật số lượng (đảm bảo > 0)
                item.Quantity = quantity > 0 ? quantity : 1;
                SaveCart(cart);
            }

            // Tính toán lại các con số để trả về cho Client
            var cartTotal = cart.Sum(x => x.Total);

            return Json(new
            {
                success = true,
                quantity = item.Quantity,
                itemTotal = item.Total.ToString("#,##0") + " đ",
                cartTotal = cartTotal.ToString("#,##0") + " đ"
            });
        }
        [HttpPost]
        
        private List<CartItemVM> GetCart()
        {
            /// nhãn giỏ hàng
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
