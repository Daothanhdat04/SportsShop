using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;
using SportsShop.Models;

namespace SportsShop.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra đã review chưa
            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == user.Id);

            if (existingReview != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá sản phẩm này rồi!";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Kiểm tra đã mua sản phẩm chưa
            var hasPurchased = await _context.Orders
                .Include(o => o.OrderDetails)
                .AnyAsync(o => o.CustomerEmail == user.Email
                          && o.OrderDetails.Any(od => od.ProductId == productId)
                          && o.Status == "Đã giao");

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment,
                IsVerifiedPurchase = hasPurchased,
                IsApproved = true // Auto approve, hoặc set false để admin duyệt
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        // POST: Reviews/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null || review.UserId != user.Id)
            {
                return NotFound();
            }

            review.Rating = rating;
            review.Comment = comment;
            review.CreatedDate = DateTime.Now; // Cập nhật thời gian

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật đánh giá thành công!";
            return RedirectToAction("Details", "Products", new { id = review.ProductId });
        }

        // POST: Reviews/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null || review.UserId != user.Id)
            {
                return NotFound();
            }

            var productId = review.ProductId;
            _context.ProductReviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa đánh giá thành công!";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        // POST: Reviews/Report (Báo cáo review spam/không phù hợp)
        [HttpPost]
        public async Task<IActionResult> Report(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                // TODO: Thêm logic báo cáo (gửi email cho admin, đánh dấu...)
                TempData["SuccessMessage"] = "Đã gửi báo cáo. Cảm ơn bạn!";
            }

            return RedirectToAction("Details", "Products", new { id = review?.ProductId });
        }
    }
}