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
    public class OrderStatusController : Controller
    {
        private readonly QlbhtContext _context;

        public OrderStatusController(QlbhtContext context)
        {
            _context = context;
        }

        // GET: Admin/OrderStatus
        // ✅ ĐÃ SỬA: Lấy dữ liệu từ biến qlbhtContext sau khi lọc để tìm kiếm hoạt động chuẩn
        public async Task<IActionResult> Index(string q)
        {
            if (!Function.IsLogin())
                return RedirectToAction("Index", "Login");

            var qlbhtContext = _context.OrderStatuses.AsQueryable();

            // Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim();
                qlbhtContext = qlbhtContext.Where(p => p.OrderStatusName.Contains(q));
                ViewBag.Keyword = q; // Gửi từ khóa lại View để hiển thị trong ô nhập
            }

            // Trả ra danh sách đã được lọc tìm kiếm
            return View(await qlbhtContext.ToListAsync());
        }

        // GET: Admin/OrderStatus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(m => m.OrderStatusId == id);
            if (orderStatus == null)
            {
                return NotFound();
            }

            return View(orderStatus);
        }

        // GET: Admin/OrderStatus/Create
        public IActionResult Create()
        {
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName");
            return View();
        }

        // POST: Admin/OrderStatus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ✅ ĐÃ SỬA: Thêm IsActive vào [Bind] để khi tạo mới không bị gán bằng False
        public async Task<IActionResult> Create([Bind("OrderStatusId,OrderStatusName,IsActive")] OrderStatus orderStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName", orderStatus.OrderStatusId);
            return View(orderStatus);
        }

        // GET: Admin/OrderStatus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderStatus = await _context.OrderStatuses.FindAsync(id);
            if (orderStatus == null)
            {
                return NotFound();
            }
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName", orderStatus.OrderStatusId);
            return View(orderStatus);
        }

        // POST: Admin/OrderStatus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ✅ ĐÃ SỬA: Thêm IsActive vào [Bind] để khi sửa thông tin không bị mất trạng thái Xóa mềm
        public async Task<IActionResult> Edit(int id, [Bind("OrderStatusId,OrderStatusName,IsActive")] OrderStatus orderStatus)
        {
            if (id != orderStatus.OrderStatusId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderStatusExists(orderStatus.OrderStatusId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName", orderStatus.OrderStatusId);
            return View(orderStatus);
        }

        // GET: Admin/OrderStatus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(m => m.OrderStatusId == id);
            if (orderStatus == null)
            {
                return NotFound();
            }

            return View(orderStatus);
        }

        // POST: Admin/OrderStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // ✅ ĐÃ SỬA: Đảm bảo luồng Xóa mềm chạy chuẩn, không lỗi chính tả
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderStatus = await _context.OrderStatuses.FindAsync(id);
            if (orderStatus != null)
            {
                orderStatus.IsActive = false; // Lật cờ xóa mềm

                _context.OrderStatuses.Update(orderStatus);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderStatusExists(int id)
        {
            return _context.OrderStatuses.Any(e => e.OrderStatusId == id);
        }
    }
}