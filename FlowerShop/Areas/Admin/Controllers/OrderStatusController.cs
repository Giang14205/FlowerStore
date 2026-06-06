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
        public async Task<IActionResult> Index(string q)
        {
            if (!Function.IsLogin())
                return RedirectToAction("Index", "Login");
            var qlbhtContext = _context.OrderStatuses.Include(m => m.OrderStatusId).Include(m => m.OrderStatusName).AsQueryable();
       

            // 3. Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim();
                qlbhtContext = qlbhtContext.Where(p => p.OrderStatusName.Contains(q));
                ViewBag.Keyword = q; // Gửi từ khóa lại View để hiển thị trong ô nhập
            }

           
            return View(await _context.OrderStatuses.ToListAsync());
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
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses , "OrderStatusId", "OrderStatusId");
            return View();
        }

        // POST: Admin/OrderStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderStatusId,OrderStatusName")] OrderStatus orderStatus)
        {
            if (ModelState.IsValid)
            {   

                _context.Add(orderStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusId", orderStatus.OrderStatusId);
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
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusId",orderStatus.OrderStatusId);
            return View(orderStatus);
        }

        // POST: Admin/OrderStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderStatusId,OrderStatusName")] OrderStatus orderStatus)
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
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusId", orderStatus.OrderStatusId);
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderStatus = await _context.OrderStatuses.FindAsync(id);
            if (orderStatus != null)
            {
                _context.OrderStatuses.Remove(orderStatus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderStatusExists(int id)
        {
            return _context.OrderStatuses.Any(e => e.OrderStatusId == id);
        }
    }
}
