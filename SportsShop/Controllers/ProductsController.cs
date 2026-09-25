using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;
using SportsShop.Models;
using SportsShop.ViewModels;
using X.PagedList;
using X.PagedList.EF;

namespace SportsShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products (cho user xem)
        public async Task<IActionResult> Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSort = sortOrder;

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString)
                    || p.Description.Contains(searchString));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice);
            }

            productsQuery = sortOrder switch
            {
                "name_asc" => productsQuery.OrderBy(p => p.Name),
                "name_desc" => productsQuery.OrderByDescending(p => p.Name),
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "newest" => productsQuery.OrderByDescending(p => p.Id),
                _ => productsQuery.OrderBy(p => p.Name)
            };

            int pageSize = 9;
            int pageNumber = page ?? 1;
            var pagedProducts = await productsQuery.ToPagedListAsync(pageNumber, pageSize);

            var viewModel = new ProductSearchViewModel
            {
                Products = pagedProducts,
                SearchString = searchString,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                Categories = await _context.Categories.ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
         .Include(p => p.Category)
         .Include(p => p.Variants)
         .Include(p => p.Images)
         .Include(p => p.Reviews) // ✅ THÊM DÒNG NÀY
             .ThenInclude(r => r.User) // Load thông tin user
         .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();  
            }

            return View(product);
        }
    }
}