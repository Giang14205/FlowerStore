using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using FlowerShop.Models;
using FlowerShop.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlowerShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StatisticalController : Controller
    {
        private readonly QlbhtContext _context;

        public StatisticalController(QlbhtContext context)
        {
            _context = context;
        }

        // GET: Admin/Statistical
        public async Task<IActionResult> Index()
        {
            // 1. Kiểm tra đăng nhập
            if (!Function.IsLogin())
                return RedirectToAction("Index", "Login");

            // ==========================================================
            // 2. BỘ KPIs TÀI CHÍNH THÔNG MINH
            // ==========================================================
            int totalOrders = await _context.Orders.CountAsync();
            int completedOrders = await _context.Orders.CountAsync(o => o.OrderStatusId == 5);
            int canceledOrders = await _context.Orders.CountAsync(o => o.OrderStatusId == 6); // Trạng thái hủy đơn

            // Chỉ tính tiền từ những đơn ĐÃ GIAO THÀNH CÔNG
            var validOrdersQuery = _context.Orders
                .Where(o => o.OrderStatusId == 5 && o.OrderDate != null);

            decimal totalRevenue = await validOrdersQuery.SumAsync(o => o.TotalAmount);

            // Giá trị đơn hàng trung bình (AOV)
            decimal averageOrderValue = completedOrders > 0 ? totalRevenue / completedOrders : 0;

            // Tỷ lệ hủy đơn và tỷ lệ thành công
            double cancelRate = totalOrders > 0 ? ((double)canceledOrders / totalOrders) * 100 : 0;
            double successRate = totalOrders > 0 ? ((double)completedOrders / totalOrders) * 100 : 0;

            // Đếm tổng số khách hàng (RoleId = 2) dựa trên bảng UserRoles kết hợp
            int totalCustomers = await _context.Users
                .CountAsync(u => _context.UserRoles.Any(ur => ur.UserId == u.UserId && ur.RoleId == 2));


            // ==========================================================
            // 3. THỐNG KÊ XU HƯỚNG 7 NGÀY GẦN NHẤT (BIỂU ĐỒ ĐƯỜNG)
            // ==========================================================
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(date => date)
                .ToList();

            DateTime sevenDaysAgo = DateTime.Today.AddDays(-6);

            var ordersInWeek = await validOrdersQuery
                .Where(o => o.OrderDate >= sevenDaysAgo)
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            var rawRevenueData = ordersInWeek
                .GroupBy(o => o.OrderDate.Value.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(o => o.TotalAmount) })
                .ToList();

            string[] chartLabels = new string[7];
            decimal[] chartValues = new decimal[7];

            for (int i = 0; i < last7Days.Count; i++)
            {
                var currentDataDate = last7Days[i];
                chartLabels[i] = currentDataDate.ToString("dd/MM");
                var match = rawRevenueData.FirstOrDefault(r => r.Date == currentDataDate);
                chartValues[i] = match != null ? match.Amount : 0;
            }


            // ==========================================================
            // 4. BIỂU ĐỒ TRÒN: CƠ CẤU DOANH THU THEO DANH MỤC LOẠI HOA
            // ==========================================================
            // Map chuẩn xác theo thực thể OrderItems, ProductCategoryName và UnitPrice trong DB của bạn
            var categoryData = await _context.OrderItems
                .Where(oi => oi.Order.OrderStatusId == 5)
                .GroupBy(oi => oi.Product.ProductCategory.ProductCategoryName)
                .Select(g => new
                {
                    CategoryName = g.Key ?? "Chưa phân loại",
                    TotalSales = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .ToListAsync();

            var pieLabels = categoryData.Select(c => c.CategoryName).ToArray();
            var pieValues = categoryData.Select(c => c.TotalSales).ToArray();


            // ==========================================================
            // 5. MÔ PHỎNG & DỰ BÁO DOANH THU CUỐI THÁNG
            // ==========================================================
            DateTime today = DateTime.Today;
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            int currentDay = today.Day;

            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            decimal currentMonthRevenue = await validOrdersQuery
                .Where(o => o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth)
                .SumAsync(o => o.TotalAmount);

            decimal forecastedRevenue = currentDay > 0 ? (currentMonthRevenue / currentDay) * daysInMonth : 0;

            decimal monthTarget = 50000000; // KPI Mục tiêu 50 triệu
            double completionRate = monthTarget > 0 ? (double)(currentMonthRevenue / monthTarget) * 100 : 0;
            if (completionRate > 100) completionRate = 100;


            // ==========================================================
            // 6. TOP 5 SẢN PHẨM BÁN CHẠY NHẤT (MÃ THEO ORDERITEMS)
            // ==========================================================
            var topProducts = await _context.OrderItems
                .Where(oi => oi.Order.OrderStatusId == 5)
                .GroupBy(oi => new { oi.ProductId, oi.Product.ProductName, oi.Product.ImagePopular })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    Image = g.Key.ImagePopular, // Cột chứa ảnh đại diện sản phẩm của bạn
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.TotalQuantity)
                .Take(5)
                .ToListAsync();


            // ==========================================================
            // 7. TOP 5 "KHÁCH RUỘT" VIP CHI TIÊU MẠNH NHẤT
            // ==========================================================
            var topCustomers = await _context.Orders
                .Where(o => o.OrderStatusId == 5)
                .GroupBy(o => new { o.UserId, o.FullName }) // Sử dụng cột FullName trực tiếp trên bảng Order để tránh lỗi dữ liệu User null
                .Select(g => new
                {
                    FullName = g.Key.FullName ?? "Khách vãng lai",
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(5)
                .ToListAsync();


            // ==========================================================
            // 8. ĐẨY TẤT CẢ DỮ LIỆU SANG GIAO DIỆN VIEW
            // ==========================================================
            // Nhóm KPIs tài chính
            ViewBag.TotalOrders = totalOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.AverageOrderValue = averageOrderValue;
            ViewBag.CancelRate = Math.Round(cancelRate, 1);
            ViewBag.SuccessRate = Math.Round(successRate, 1);

            // Nhóm Dự báo mục tiêu
            ViewBag.CurrentMonthRevenue = currentMonthRevenue;
            ViewBag.ForecastedRevenue = forecastedRevenue;
            ViewBag.MonthTarget = monthTarget;
            ViewBag.CompletionRate = Math.Round(completionRate, 1);
            ViewBag.CurrentMonthName = today.ToString("MM/yyyy");

            // Nhóm Danh sách hiển thị bảng dữ liệu thô
            ViewBag.TopProducts = topProducts;
            ViewBag.TopCustomers = topCustomers;

            // Nhóm Chuỗi JSON vẽ Biểu đồ đường & Biểu đồ tròn
            ViewBag.ChartLabels = Newtonsoft.Json.JsonConvert.SerializeObject(chartLabels);
            ViewBag.ChartValues = Newtonsoft.Json.JsonConvert.SerializeObject(chartValues);
            ViewBag.PieLabels = Newtonsoft.Json.JsonConvert.SerializeObject(pieLabels);
            ViewBag.PieValues = Newtonsoft.Json.JsonConvert.SerializeObject(pieValues);

            return View();
        }
        // file excel
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            // 1. Lấy dữ liệu Doanh thu theo tháng
            var monthlyData = await _context.Orders
                .Where(o => o.OrderStatusId == 5 && o.OrderDate.HasValue)
                .GroupBy(o => new { o.OrderDate.Value.Month, o.OrderDate.Value.Year })
                .Select(g => new { Thang = g.Key.Month, Nam = g.Key.Year, DoanhThu = g.Sum(o => o.TotalAmount) })
                .OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang)
                .ToListAsync();

            // 2. Lấy dữ liệu Top sản phẩm
            var topProducts = await _context.OrderItems
                .Where(oi => oi.Order.OrderStatusId == 5)
                .GroupBy(oi => new { oi.ProductId, oi.Product.ProductName })
                .Select(g => new { ProductName = g.Key.ProductName, TotalQuantity = g.Sum(oi => oi.Quantity), TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice) })
                .OrderByDescending(p => p.TotalQuantity).Take(5).ToListAsync();

            // 3. Lấy dữ liệu Top khách hàng
            var topCustomers = await _context.Orders
                .Where(o => o.OrderStatusId == 5)
                .GroupBy(o => new { o.UserId, o.FullName })
                .Select(g => new { FullName = g.Key.FullName ?? "Khách vãng lai", TotalOrders = g.Count(), TotalSpent = g.Sum(o => o.TotalAmount) })
                .OrderByDescending(c => c.TotalSpent).Take(5).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                // --- Sheet 1: Doanh Thu Theo Tháng ---
                var ws1 = workbook.Worksheets.Add("Doanh Thu Theo Thang");
                ws1.Cell(1, 1).Value = "Tháng"; ws1.Cell(1, 2).Value = "Năm"; ws1.Cell(1, 3).Value = "Doanh Thu";
                for (int i = 0; i < monthlyData.Count; i++)
                {
                    ws1.Cell(i + 2, 1).Value = monthlyData[i].Thang;
                    ws1.Cell(i + 2, 2).Value = monthlyData[i].Nam;
                    ws1.Cell(i + 2, 3).Value = monthlyData[i].DoanhThu;
                    ws1.Cell(i + 2, 3).Style.NumberFormat.Format = "#,##0 \"đ\"";
                }
                ws1.Columns().AdjustToContents();

                // --- Sheet 2: Top Sản Phẩm ---
                var ws2 = workbook.Worksheets.Add("Top San Pham");
                ws2.Cell(1, 1).Value = "Tên Sản Phẩm"; ws2.Cell(1, 2).Value = "Số Lượng Bán"; ws2.Cell(1, 3).Value = "Doanh Thu";
                for (int i = 0; i < topProducts.Count; i++)
                {
                    ws2.Cell(i + 2, 1).Value = topProducts[i].ProductName;
                    ws2.Cell(i + 2, 2).Value = topProducts[i].TotalQuantity;
                    ws2.Cell(i + 2, 3).Value = topProducts[i].TotalRevenue;
                    ws2.Cell(i + 2, 3).Style.NumberFormat.Format = "#,##0 \"đ\"";
                }
                ws2.Columns().AdjustToContents();

                // --- Sheet 3: Top Khách Hàng ---
                var ws3 = workbook.Worksheets.Add("Top Khach Hang");
                ws3.Cell(1, 1).Value = "Tên Khách Hàng"; ws3.Cell(1, 2).Value = "Số Đơn"; ws3.Cell(1, 3).Value = "Tổng Chi Tiêu";
                for (int i = 0; i < topCustomers.Count; i++)
                {
                    ws3.Cell(i + 2, 1).Value = topCustomers[i].FullName;
                    ws3.Cell(i + 2, 2).Value = topCustomers[i].TotalOrders;
                    ws3.Cell(i + 2, 3).Value = topCustomers[i].TotalSpent;
                    ws3.Cell(i + 2, 3).Style.NumberFormat.Format = "#,##0 \"đ\"";
                }
                ws3.Columns().AdjustToContents();

                // Xuất file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoTongHop_{DateTime.Now:ddMMyyyy}.xlsx");
                }
            }
        }
    }
}