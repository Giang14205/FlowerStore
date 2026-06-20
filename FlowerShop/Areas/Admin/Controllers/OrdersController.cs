using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FlowerShop.Models;
using FlowerShop.Utilities;
namespace FlowerShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly QlbhtContext _context;

        public OrdersController(QlbhtContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            if (!Function.IsLogin())
                return RedirectToAction("Index", "Login");
            
            var qlbhtContext = _context.Orders.Include(o => o.OrderStatus).Include(o => o.PayMethod).Include(o => o.User).Include(o => o.Voucher).OrderByDescending(o => o.OrderDate);
            return View(await qlbhtContext.ToListAsync());
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.PayMethod)
                .Include(o => o.User)
                .Include(o => o.Voucher)
               .Include(o => o.OrderItems)          
               .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "FullName");
            ViewBag.VoucherId = new SelectList(_context.Vouchers, "VoucherId", "VoucherName");
            ViewBag.PayMethodId = new SelectList(_context.PayMethods, "PayMethodId", "PayMethodName");
            ViewBag.OrderStatusId = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName");
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        // 🔥 ĐÃ THÊM: Hứng thêm biến int SelectedProductId gửi lên từ thẻ select ngoài View
        public async Task<IActionResult> Create([Bind("OrderId,UserId,ShippingAddress,OrderNumber,VoucherId,TotalAmount,ShippingAmount,PayMethodId,OrderStatusId,OrderDate,FullName,Phone,Address,Note")] Order order, int SelectedProductId)
        {
            ModelState.Remove("User");
            ModelState.Remove("PayMethod");
            ModelState.Remove("OrderStatus");

            if (ModelState.IsValid)
            {
                // 1. Lưu thông tin tổng của đơn hàng vào bảng Order trước để lấy OrderId
                _context.Add(order);
                await _context.SaveChangesAsync();

                // 2. TỰ ĐỘNG TẠO CHI TIẾT ĐƠN HÀNG (Nếu Admin có chọn sản phẩm)
                if (SelectedProductId > 0)
                {
                    // Lấy giá tiền thực tế của sản phẩm từ DB để lưu cho chuẩn bảo mật
                    var product = await _context.Products.FindAsync(SelectedProductId);
                    if (product != null)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,             // Lấy mã đơn hàng vừa sinh ở trên
                            ProductId = SelectedProductId,       // Mã sản phẩm Admin chọn
                            Quantity = 1,                        // Mặc định tạo tay là 1 bó (hoặc Giang tự thêm ô số lượng tùy ý)
                            UnitPrice = product.PriceSale,       // Đơn giá gốc của hoa
                            LineTotal = product.ProductPrice * 1,    // Thành tiền dòng
                            Note = "Sản phẩm tạo bởi Admin"
                        };
                        _context.OrderItems.Add(orderItem);
                        await _context.SaveChangesAsync();       // Lưu chính thức vào bảng OrderItem
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Nếu dữ liệu lỗi, nạp lại toàn bộ dữ liệu ra View để không bị lỗi giao diện
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "FullName", order.UserId);
            ViewBag.VoucherId = new SelectList(_context.Vouchers, "VoucherId", "VoucherName", order.VoucherId);
            ViewBag.PayMethodId = new SelectList(_context.PayMethods, "PayMethodId", "PayMethodName", order.PayMethodId);
            ViewBag.OrderStatusId = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName", order.OrderStatusId);
            ViewBag.Products = _context.Products.ToList();
            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.PayMethod)
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderItems)          
                    .ThenInclude(oi => oi.Product)  
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "FullName", order.UserId);
            ViewBag.VoucherId = new SelectList(_context.Vouchers, "VoucherId", "VoucherName", order.VoucherId);
            ViewBag.PayMethodId = new SelectList(_context.PayMethods, "PayMethodId", "PayMethodName", order.PayMethodId);
            ViewBag.OrderStatusId = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName", order.OrderStatusId);
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            // 1. Ép ép loại bỏ kiểm tra Validation cho các đối tượng liên kết phức tạp
            ModelState.Remove("OrderItems");
            ModelState.Remove("User");
            ModelState.Remove("OrderStatus");
            ModelState.Remove("PayMethod");
            ModelState.Remove("Voucher");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    // THÀNH CÔNG: Sẽ nhảy thẳng về trang quản lý danh sách đơn
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // ====================================================================
            // 🚨 KHU VỰC CỨU DỮ LIỆU: Nếu đen đủi bị dính lỗi Validation, 
            // phải nạp lại toàn bộ hoa lẻ và ảnh Base64 lên để không bị TRẮNG GIAO DIỆN!
            // ====================================================================

            // Nạp lại mớ hoa lẻ và ảnh mẫu từ CSDL bọc ngược vào đối tượng trước khi trả ra View
            order.OrderItems = await _context.OrderItems
                .Where(x => x.OrderId == order.OrderId)
                .Include(x => x.Product)
                .ToListAsync();

            // Nạp lại toàn bộ danh sách dropdown list để giao diện không bị crash
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "FullName", order.UserId);
            ViewBag.VoucherId = new SelectList(_context.Vouchers, "VoucherId", "VoucherName", order.VoucherId);
            ViewBag.PayMethodId = new SelectList(_context.PayMethods, "PayMethodId", "PayMethodName", order.PayMethodId);
            ViewBag.OrderStatusId = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName", order.OrderStatusId);

            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.PayMethod)
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        // ====================================================================
        // 🔥 SỬA LẠI HÀM DELETE POST ĐỂ KHÔNG BỊ CHẶN KHÓA NGOẠI SQL SERVER
        // ====================================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Tìm đơn hàng cần xử lý kèm theo trạng thái hiện tại
            var order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                // 💡 Giả định ID của trạng thái "Hủy đơn" trong bảng OrderStatus của má là 5 
                // (Giang check lại trong DB xem mã Trạng thái Hủy đơn của má là số mấy thì sửa lại số này nha)
                int idTrangThaiHuyDon = 6;

                // KỊCH BẢN A: Nếu đơn hàng là đơn thật (chưa ở trạng thái Hủy), ta chỉ XÓA MỀM (Chuyển trạng thái)
                if (order.OrderStatusId != idTrangThaiHuyDon && order.Note != "Sản phẩm tạo bởi Admin")
                {
                    order.OrderStatusId = idTrangThaiHuyDon; // Hạ biển đơn sang trạng thái Hủy
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                // KỊCH BẢN B: Nếu đã là đơn Hủy rồi, hoặc là đơn do Admin tạo tay để test hệ thống -> TIẾN HÀNH XÓA CỨNG
                else
                {
                    // Tìm toàn bộ các sản phẩm, hoa lẻ (con) thuộc đơn hàng này để dọn đường khóa ngoại
                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.OrderId == id)
                        .ToListAsync();

                    if (orderItems.Any())
                    {
                        _context.OrderItems.RemoveRange(orderItems);
                        await _context.SaveChangesAsync(); // Dọn sạch bảng con trước
                    }

                    // Xóa sổ vỏ đơn hàng (Cha) khỏi hệ thống vĩnh viễn
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                }
            }

            // Xử lý xong quay về trang danh sách quản lý đơn hàng
            return RedirectToAction(nameof(Index));
        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
