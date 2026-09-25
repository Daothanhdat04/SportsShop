using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShop.Models;
using SportsShop.ViewModels;
using SportsShop.Data;

namespace SportsShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string searchString)
        {
            ViewBag.SearchString = searchString;

            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.FullName.Contains(searchString)
                    || u.Email.Contains(searchString)
                    || u.PhoneNumber.Contains(searchString));
            }

            var userList = await users.OrderBy(u => u.FullName).ToListAsync();

            // Lấy role cho từng user
            var userWithRoles = new List<(ApplicationUser User, string Role)>();
            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "No Role";
                userWithRoles.Add((user, role));
            }

            return View(userWithRoles);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            return View(user);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model, string role)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Thêm role
                    if (!string.IsNullOrEmpty(role))
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }

                    TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new RegisterViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = roles.FirstOrDefault();

            return View(model);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RegisterViewModel model, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Cập nhật role
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!string.IsNullOrEmpty(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }

                TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra người dùng có đơn hàng không (kiểm tra qua email)
            var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerEmail == user.Email);

            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng này vì có đơn hàng đang tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra người dùng có đánh giá sản phẩm không
            var hasReviews = await _context.ProductReviews.AnyAsync(r => r.UserId == id);

            if (hasReviews)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng này vì có đánh giá sản phẩm đang tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không có ràng buộc, tiến hành xóa
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Xóa người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Không thể xóa người dùng!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Users/ResetPassword/5
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Reset password về mặc định: User@123
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, "User@123");

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Reset mật khẩu thành công! Mật khẩu mới: User@123";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["ErrorMessage"] = "Không thể reset mật khẩu!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}