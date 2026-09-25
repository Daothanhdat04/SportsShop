using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;
using SportsShop.Helpers;
using SportsShop.Models;

namespace SportsShop.Controllers
{


    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "CartSession";
        private readonly UserManager<ApplicationUser> _userManager;


        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Trang thanh toán
        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);

            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống! Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Products");
            }

            ViewBag.Total = cart.Sum(item => item.TotalPrice);
            ViewBag.CartItems = cart;

            // Tự động điền thông tin nếu user đã đăng nhập
            var order = new Order();
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    order.CustomerName = user.FullName;
                    order.CustomerEmail = user.Email;
                    order.CustomerPhone = user.PhoneNumber;
                    order.ShippingAddress = user.Address;
                }
            }

            return View(order);
        }
        

        // Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);

            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Products");
            }

            // Kiểm tra tồn kho
            foreach (var item in cart)
            {
                if (item.VariantId.HasValue)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId.Value);
                    if (variant == null || variant.Stock < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm {item.ProductName} không đủ hàng!";
                        return RedirectToAction("Index", "Cart");
                    }
                }
                else
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm {item.ProductName} không đủ hàng!";
                        return RedirectToAction("Index", "Cart");
                    }
                }
            }

            // Tạo đơn hàng
            order.OrderDate = DateTime.Now;
            order.TotalAmount = cart.Sum(item => item.TotalPrice);
            order.Status = "Đang xử lý";

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng
            foreach (var item in cart)
            {
                // ✅ LẤY THÔNG TIN SẢN PHẨM
                var product = await _context.Products.FindAsync(item.ProductId);

                // ✅ XÂY DỰNG THÔNG TIN BIẾN THỂ
                string variantInfo = "";
                if (item.VariantId.HasValue)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId.Value);
                    if (variant != null)
                    {
                        variantInfo = $"{variant.Color} - {variant.Size}";
                    }
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,

                    // ✅ LƯU THÔNG TIN SẢN PHẨM
                    ProductName = product?.Name ?? item.ProductName,
                    ProductImageUrl = product?.ImageUrl,
                    VariantInfo = variantInfo
                };

                _context.OrderDetails.Add(orderDetail);

                // Trừ tồn kho
                if (item.VariantId.HasValue)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId.Value);
                    if (variant != null)
                    {
                        variant.Stock -= item.Quantity;
                    }
                }
                else
                {
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Xóa giỏ hàng
            HttpContext.Session.Remove(CartSessionKey);

            TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: #{order.Id}";
            return RedirectToAction("OrderSuccess", new { id = order.Id });
        }

        // Trang xác nhận đơn hàng thành công
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}