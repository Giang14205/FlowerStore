using FlowerShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FlowerShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly QlbhtContext _context;

        // 1. Chỉ Inject Database (đã bỏ IVnPayService vì không còn dùng ở đây nữa)
        public ShoppingCartController(QlbhtContext context)
        {
            _context = context;
        }

        // 2. Trang chủ giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // 3. Thêm vào giỏ hàng
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

        // 4. Xóa khỏi giỏ hàng
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

        // 5. Cập nhật số lượng giỏ hàng (Gọi bằng AJAX)
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