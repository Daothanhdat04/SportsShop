using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;
using SportsShop.Helpers;
using SportsShop.Models;

namespace SportsShop.Controllers
{
    [Authorize] // THÊM DÒNG NÀY - Bắt buộc đăng nhập cho toàn bộ controller

    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "CartSession";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            ViewBag.Total = cart.Sum(item => item.TotalPrice);
            return View(cart);
        }

        // Thêm vào giỏ hàng
        //public async Task<IActionResult> AddToCart(int id)
        //{
        //    var product = await _context.Products.FindAsync(id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

        //    var cartItem = cart.FirstOrDefault(c => c.ProductId == id);
        //    if (cartItem != null)
        //    {
        //        cartItem.Quantity++;
        //    }
        //    else
        //    {
        //        cart.Add(new CartItem
        //        {
        //            ProductId = product.Id,
        //            ProductName = product.Name,
        //            Price = product.Price,
        //            Quantity = 1,
        //            ImageUrl = product.ImageUrl
        //        });
        //    }

        //    HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        //    TempData["SuccessMessage"] = $"Đã thêm {product.Name} vào giỏ hàng!";

        //    return RedirectToAction("Index", "Products");
        //}
        // Thêm vào giỏ hàng với giá giảm
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.DisplayPrice, // SỬ DỤNG GIÁ SAU GIẢM
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            TempData["SuccessMessage"] = $"Đã thêm {product.Name} vào giỏ hàng!";

            return RedirectToAction("Index", "Cart");
        }

        // Xóa khỏi giỏ hàng
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            }

            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng!";
            return RedirectToAction("Index");
        }
        // Thêm vào giỏ hàng với size và màu
        [HttpPost]
        public async Task<IActionResult> AddToCartWithVariant(int productId, string size, string color, int quantity = 1)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Tìm variant phù hợp
            var variant = product.Variants?.FirstOrDefault(v => v.Size == size && v.Color == color);

            if (variant == null || variant.Stock < quantity)
            {
                TempData["ErrorMessage"] = "Sản phẩm với size và màu này không đủ hàng!";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            // Kiểm tra xem đã có sản phẩm này với size/màu trong giỏ chưa
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size && c.Color == color);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                // ✅ TÍNH GIÁ: Ưu tiên giá giảm của variant, nếu không có thì lấy giá giảm của product
                decimal finalPrice = product.Price; // Giá gốc mặc định

                if (variant.HasDiscount)
                {
                    finalPrice = variant.DiscountPrice.Value; // Giá giảm riêng của variant
                }
                else if (product.IsOnDiscount)
                {
                    finalPrice = product.DisplayPrice; // Giá giảm chung của product
                }

                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = finalPrice, // ✅ GIÁ SAU GIẢM
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl,
                    Size = size,
                    Color = color,
                    VariantId = variant.Id
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            TempData["SuccessMessage"] = $"Đã thêm {product.Name} ({size} - {color}) vào giỏ hàng!";

            return RedirectToAction("Index");
        }
    }
}