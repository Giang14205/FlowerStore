using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims; // 🔥 ĐÃ THÊM: Để bốc được Id của người dùng đang đăng nhập
using FlowerShop.Controllers;
namespace FlowerShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly QlbhtContext _context;

        public ShoppingCartController(QlbhtContext context)
        {
            _context = context;
        }

        // 2. Trang chủ giỏ hàng - ĐÃ SỬA: Gom cả hàng Session và hàng Database phối lẻ
        public async Task<IActionResult> Index()
        {
            // A. Lấy Id của người dùng đăng nhập hệ thống
            int userId = 0;
            var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimUserId))
            {
                userId = int.Parse(claimUserId);
            }

            // Tạo danh sách vạn năng sử dụng lớp đệm CheckoutItemDto (hoặc Giang lấy trực tiếp từ file CheckoutController)
            var unifiedCart = new List<CheckoutController.CheckoutItemDto>();

            // LUỒNG 1: Bốc hàng thường từ Session "GioHang"
            var sessionCart = GetCart();
            foreach (var item in sessionCart)
            {
                unifiedCart.Add(new CheckoutController.CheckoutItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = (decimal)item.Price,
                    Note = "Sản phẩm cửa hàng",
                    ProductImage = item.Image // 🔥 CHUẨN ĐÉT: item.Image lấy từ CartItemVM ra
                });
            }

            // LUỒNG 2: Bốc hoa lẻ phối từ bảng CartItems dưới Database (Nếu khách đã đăng nhập)
            if (userId > 0)
            {
                var dbCartItems = await _context.CartItems
                    .Include(x => x.Product)
                    .Where(x => x.Cart.UserId == userId).ToListAsync();

                foreach (var item in dbCartItems)
                {
                    unifiedCart.Add(new CheckoutController.CheckoutItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product?.ProductName ?? "Nguyên liệu lẻ",
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Note = item.Note, // Vết tích chuỗi ảnh chụp Base64 "Bó tự phối...|data:..."
                        ProductImage = item.Product?.ImagePopular ?? "/images/default.png"
                    });
                }
            }

            // Tính tổng chi phí tổng thể (Toàn bộ hoa thường + Toàn bộ hoa phối)
            var customBouquetCount = unifiedCart
                .Where(x => x.Note != null && x.Note.StartsWith("Bó tự phối"))
                .Select(x => x.Note)
                .Distinct()
                .Count();

            decimal totalItemsPrice = unifiedCart.Sum(x => x.Quantity * x.Price);
            decimal finalTotal = totalItemsPrice + (customBouquetCount * 50000); // Công cắm 50k cho mỗi bó tự phối

            ViewBag.Total = finalTotal;

            // Ném cái danh sách đã gộp sang cho View hiển thị
            return View(unifiedCart);
        }

        // 3. Thêm vào giỏ hàng (Giữ nguyên)
        public IActionResult AddToCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);

            if (item != null)
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

        // 4. Xóa khỏi giỏ hàng (Giữ nguyên)
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // 5. Cập nhật số lượng giỏ hàng (Giữ nguyên)
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity > 0 ? quantity : 1;
                SaveCart(cart);
            }

            var cartTotal = cart.Sum(x => x.Total);

            return Json(new
            {
                success = true,
                quantity = item.Quantity,
                itemTotal = item.Total.ToString("#,##0") + " đ",
                cartTotal = cartTotal.ToString("#,##0") + " đ"
            });
        }

        // --- CÁC HÀM HỖ TRỢ XỬ LÝ SESSION GIỎ HÀNG (Giữ nguyên) ---
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
        public async Task<IActionResult> RemoveCustomBouquet(string bouquetNote)
        {
            // Lấy mã người dùng đăng nhập
            var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimUserId))
            {
                int userId = int.Parse(claimUserId);

                // Tìm toàn bộ các bông hoa lẻ mang chung cái mã Note (bouquetNote) của cơ thể bó hoa này dưới DB
                var itemsToRemove = await _context.CartItems
                    .Where(x => x.Cart.UserId == userId && x.Note == bouquetNote)
                    .ToListAsync();

                if (itemsToRemove.Any())
                {
                    // Xóa sạch cả cụm bản ghi đó cùng một lúc luôn
                    _context.CartItems.RemoveRange(itemsToRemove);
                    await _context.SaveChangesAsync();
                }
            }

            // Xóa xong tải lại trang giỏ hàng là sạch bóng quân thù
            return RedirectToAction("Index");
        }
    }
}