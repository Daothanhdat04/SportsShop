using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;
using SportsShop.Models;
using System.Diagnostics;

namespace SportsShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
                ViewBag.SelectedCategory = await _context.Categories.FindAsync(categoryId);
            }

            var products = await productsQuery
                .OrderByDescending(p => p.Id)
                .Take(6)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // Trang tìm kiếm cho khách hàng
        public async Task<IActionResult> Search(string q)
        {
            ViewBag.SearchQuery = q;

            if (string.IsNullOrEmpty(q))
            {
                return View(new List<Product>());
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(q) || p.Description.Contains(q) || p.Category.Name.Contains(q))
                .Take(20)
                .ToListAsync();

            return View(products);
        }
        // Thêm Action này vào HomeController.cs

        public IActionResult About()
        {
            return View();
        }
    }
}