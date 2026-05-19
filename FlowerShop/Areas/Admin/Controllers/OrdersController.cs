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
            return View();
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,ShippingAddress,OrderNumber,VoucherId,TotalAmount,ShippingAmount,PayMethodId,OrderStatusId,OrderDate")] Order order)
        {
            ModelState.Remove("User");
            ModelState.Remove("PayMethod");
            ModelState.Remove("OrderStatus");


            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "FullName", order.UserId);
            ViewBag.VoucherId = new SelectList(_context.Vouchers, "VoucherId", "VoucherName", order.VoucherId);
            ViewBag.PayMethodId = new SelectList(_context.PayMethods, "PayMethodId", "PayMethodName", order.PayMethodId);
            ViewBag.OrderStatusId = new SelectList(_context.OrderStatuses, "OrderStatusId", "OrderStatusName", order.OrderStatusId);
            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,ShippingAddress,OrderNumber,VoucherId,TotalAmount,ShippingAmount,PayMethodId,OrderStatusId,OrderDate")] Order order)
        {
            ModelState.Remove("User");
            ModelState.Remove("PayMethod");
            ModelState.Remove("OrderStatus");

            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
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
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
