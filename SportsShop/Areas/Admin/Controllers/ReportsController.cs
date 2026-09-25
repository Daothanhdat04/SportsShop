using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;

namespace SportsShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var currentMonth = month ?? DateTime.Now.Month;

            ViewBag.Year = currentYear;
            ViewBag.Month = currentMonth;
            ViewBag.Years = Enumerable.Range(2020, DateTime.Now.Year - 2019).Reverse();
            ViewBag.Months = Enumerable.Range(1, 12);

            // Thống kê tổng quan
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status != "Đã hủy")
                .SumAsync(o => o.TotalAmount);
            ViewBag.TotalCustomers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();

            // Doanh thu theo tháng
            var monthlyRevenue = await _context.Orders
                .Where(o => o.OrderDate.Year == currentYear && o.OrderDate.Month == currentMonth && o.Status != "Đã hủy")
                .SumAsync(o => o.TotalAmount);
            ViewBag.MonthlyRevenue = monthlyRevenue;

            // Số đơn hàng theo tháng
            var monthlyOrders = await _context.Orders
                .Where(o => o.OrderDate.Year == currentYear && o.OrderDate.Month == currentMonth)
                .CountAsync();
            ViewBag.MonthlyOrders = monthlyOrders;

            // Top 10 sản phẩm bán chạy
            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => new { od.ProductId, od.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Quantity * od.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();
            ViewBag.TopProducts = topProducts;

            // Doanh thu 12 tháng gần nhất
            var last12Months = new List<string>();
            var monthlyRevenueData = new List<decimal>();

            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                last12Months.Add(date.ToString("MM/yyyy"));

                var revenue = await _context.Orders
                    .Where(o => o.OrderDate.Year == date.Year && o.OrderDate.Month == date.Month && o.Status != "Đã hủy")
                    .SumAsync(o => o.TotalAmount);
                monthlyRevenueData.Add(revenue);
            }
            ViewBag.Last12Months = last12Months;
            ViewBag.MonthlyRevenueData = monthlyRevenueData;

            // Thống kê theo danh mục
            var categoryStats = await _context.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count,
                    TotalSold = c.Products.SelectMany(p => p.OrderDetails).Sum(od => od.Quantity)
                })
                .ToListAsync();
            ViewBag.CategoryStats = categoryStats;

            // Trạng thái đơn hàng
            var orderStatusStats = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
            ViewBag.OrderStatusStats = orderStatusStats;

            // Khách hàng mua nhiều nhất
            var topCustomers = await _context.Orders
                .Where(o => o.Status != "Đã hủy")
                .GroupBy(o => new { o.CustomerName, o.CustomerEmail })
                .Select(g => new
                {
                    CustomerName = g.Key.CustomerName,
                    CustomerEmail = g.Key.CustomerEmail,
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();
            ViewBag.TopCustomers = topCustomers;

            return View();
        }
    }
}