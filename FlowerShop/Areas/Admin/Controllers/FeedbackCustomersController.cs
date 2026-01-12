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
    public class FeedbackCustomersController : Controller
    {
        private readonly QlbhtContext _context;

        public FeedbackCustomersController(QlbhtContext context)
        {
            _context = context;
        }

        // GET: Admin/FeedbackCustomers
        public async Task<IActionResult> Index(string q)
        {
            if (!Function.IsLogin())
                return RedirectToAction("Index", "Login");
            var qlbhtContext = _context.FeedbackCustomers.Include(f => f.Product).AsQueryable(); // Chuyển về dạng truy vấn để lọc thêm

            // 3. Nếu có từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim();
               qlbhtContext = qlbhtContext.Where(p => p.Content.Contains(q)
                                            || p.Ten.Contains(q));
                ViewBag.Keyword = q; // Gửi từ khóa lại View để hiển thị trong ô nhập
            }

            return View(await qlbhtContext.ToListAsync());
            
        }

        // GET: Admin/FeedbackCustomers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedbackCustomer = await _context.FeedbackCustomers
                .Include(f => f.Product)
                .FirstOrDefaultAsync(m => m.FeedbackCustomerId == id);
            if (feedbackCustomer == null)
            {
                return NotFound();
            }

            return View(feedbackCustomer);
        }

        // GET: Admin/FeedbackCustomers/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        // POST: Admin/FeedbackCustomers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FeedbackCustomerId,Content,AvatarUrl,Star,IsActive,ProductId,Ten,Email")] FeedbackCustomer feedbackCustomer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedbackCustomer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", feedbackCustomer.ProductId);
            return View(feedbackCustomer);
        }

        // GET: Admin/FeedbackCustomers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedbackCustomer = await _context.FeedbackCustomers.FindAsync(id);
            if (feedbackCustomer == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", feedbackCustomer.ProductId);
            return View(feedbackCustomer);
        }

        // POST: Admin/FeedbackCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FeedbackCustomerId,Content,AvatarUrl,Star,IsActive,ProductId,Ten,Email")] FeedbackCustomer feedbackCustomer)
        {
            if (id != feedbackCustomer.FeedbackCustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedbackCustomer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackCustomerExists(feedbackCustomer.FeedbackCustomerId))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductNames", feedbackCustomer.ProductId);
            return View(feedbackCustomer);
        }

        // GET: Admin/FeedbackCustomers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedbackCustomer = await _context.FeedbackCustomers
                .Include(f => f.Product)
                .FirstOrDefaultAsync(m => m.FeedbackCustomerId == id);
            if (feedbackCustomer == null)
            {
                return NotFound();
            }

            return View(feedbackCustomer);
        }

        // POST: Admin/FeedbackCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedbackCustomer = await _context.FeedbackCustomers.FindAsync(id);
            if (feedbackCustomer != null)
            {
                _context.FeedbackCustomers.Remove(feedbackCustomer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackCustomerExists(int id)
        {
            return _context.FeedbackCustomers.Any(e => e.FeedbackCustomerId == id);
        }
    }
}
