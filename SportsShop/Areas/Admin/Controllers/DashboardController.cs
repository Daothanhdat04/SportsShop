using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;

namespace SportsShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            // Doanh thu
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status != "Đã hủy")
                .SumAsync(o => o.TotalAmount);

            // Đơn hàng chờ xử lý
            ViewBag.PendingOrders = await _context.Orders
                .Where(o => o.Status == "Đang xử lý")
                .CountAsync();

            // Sản phẩm sắp hết hàng
            ViewBag.LowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .CountAsync();

            // Đơn hàng mới nhất
            var recentOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentOrders = recentOrders;

            // Sản phẩm bán chạy
            var topProducts = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity),
                    Product = _context.Products.FirstOrDefault(p => p.Id == g.Key)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            ViewBag.TopProducts = topProducts;

            // Doanh thu 7 ngày gần đây
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.Date.AddDays(-i))
                .Reverse()
                .ToList();

            var dailyRevenue = new List<decimal>();
            foreach (var day in last7Days)
            {
                var revenue = await _context.Orders
                    .Where(o => o.OrderDate.Date == day && o.Status != "Đã hủy")
                    .SumAsync(o => o.TotalAmount);
                dailyRevenue.Add(revenue);
            }

            ViewBag.Last7Days = last7Days.Select(d => d.ToString("dd/MM")).ToList();
            ViewBag.DailyRevenue = dailyRevenue;

            return View();
        }
    }
}