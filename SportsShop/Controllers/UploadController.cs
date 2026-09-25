using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SportsShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Không có file được chọn!" });
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif, .webp)!" });
            }

            // Kiểm tra kích thước file (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "File không được vượt quá 5MB!" });
            }

            try
            {
                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{extension}";

                // Đường dẫn lưu file
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                // Tạo folder nếu chưa có
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return URL của ảnh
                var imageUrl = $"/images/products/{fileName}";

                return Json(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi upload: {ex.Message}" });
            }
        }
    }
}