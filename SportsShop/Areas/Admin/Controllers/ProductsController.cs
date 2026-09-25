using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShop.Data;
using SportsShop.Models;
using X.PagedList;
using X.PagedList.EF;
using SportsShop.ViewModels;
using Microsoft.AspNetCore.Authorization;




namespace SportsShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            var products = await productsQuery.OrderBy(p => p.Name).ToListAsync();
            return View(products);
        }
        // GET: Admin/Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }


        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<VariantDto> VariantsList)
        {
            // DEBUG TOÀN BỘ FORM DATA
            Console.WriteLine("\n========== FULL FORM DEBUG ==========");
            Console.WriteLine($"Product Name: {product.Name}");
            Console.WriteLine($"Has Variants: {product.HasVariants}");
            Console.WriteLine($"VariantsList parameter count: {VariantsList?.Count ?? 0}");

            Console.WriteLine("\n--- ALL FORM KEYS ---");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key} = {Request.Form[key]}");
            }

            Console.WriteLine("\n--- VARIANTS FROM FORM ---");
            var formColors = Request.Form["VariantsList[0].Color"];
            var formSizes = Request.Form["VariantsList[0].Size"];
            Console.WriteLine($"VariantsList[0].Color: {formColors}");
            Console.WriteLine($"VariantsList[0].Size: {formSizes}");

            // Thử đọc thủ công
            var manualVariants = new List<ProductVariant>();
            int index = 0;
            while (Request.Form.ContainsKey($"VariantsList[{index}].Color"))
            {
                var color = Request.Form[$"VariantsList[{index}].Color"].ToString();
                var size = Request.Form[$"VariantsList[{index}].Size"].ToString();
                var stock = int.Parse(Request.Form[$"VariantsList[{index}].Stock"].ToString());
                var sku = Request.Form[$"VariantsList[{index}].SKU"].ToString();

                Console.WriteLine($"Manual read [{index}]: {color} - {size} - {stock}");

                manualVariants.Add(new ProductVariant
                {
                    Color = color,
                    Size = size,
                    Stock = stock,
                    SKU = sku
                });

                index++;
            }
            Console.WriteLine($"Manual variants count: {manualVariants.Count}");
            Console.WriteLine("=====================================\n");

            // Nếu VariantsList null, dùng manualVariants
            if (VariantsList == null || VariantsList.Count == 0)
            {
                Console.WriteLine("⚠️ Using manual variants instead!");
                // SỬ DỤNG manualVariants để lưu
            }

            if (VariantsList != null)
            {
                foreach (var v in VariantsList)
                {
                    Console.WriteLine($"Variant: {v.Color} - {v.Size} - {v.Stock}");
                }
            }

            ModelState.Remove("Variants");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Lưu Product
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Product saved: ID = {product.Id}");

                    // 2. Lưu Variants
                    if (product.HasVariants && VariantsList != null && VariantsList.Count > 0)
                    {
                        foreach (var variantDto in VariantsList)
                        {
                            var variant = new ProductVariant
                            {
                                ProductId = product.Id,
                                Color = variantDto.Color,
                                Size = variantDto.Size,
                                Stock = variantDto.Stock,
                                SKU = variantDto.SKU
                            };
                            _context.ProductVariants.Add(variant);
                            Console.WriteLine($"✅ Added variant: {variant.Color} - {variant.Size}");
                        }

                        var savedCount = await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ Saved {savedCount} variants");
                        TempData["SuccessMessage"] = $"Thêm sản phẩm và {VariantsList.Count} biến thể thành công!";
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No variants to save");
                        TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    Console.WriteLine($"Stack: {ex.StackTrace}");
                    TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine("❌ ModelState Invalid");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"{error.Key}: {err.ErrorMessage}");
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // DTO Class - THÊM CLASS NÀY NGAY TRONG CONTROLLER
        public class VariantDto
        {
            public string Color { get; set; }
            public string Size { get; set; }
            public int Stock { get; set; }
            public string SKU { get; set; }
        }

        // GET: Admin/Products/ManageVariants/5
        public async Task<IActionResult> ManageVariants(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/AddVariant
        [HttpPost]
        public async Task<IActionResult> AddVariant(int productId, string color, string size, int stock, int? discountPercent, decimal? discountPrice)
        {
            try
            {
                // ✅ DEBUG
                Console.WriteLine($"=== ADD VARIANT DEBUG ===");
                Console.WriteLine($"ProductId: {productId}");
                Console.WriteLine($"Color: {color}, Size: {size}, Stock: {stock}");
                Console.WriteLine($"DiscountPercent: {discountPercent}");
                Console.WriteLine($"DiscountPrice: {discountPrice}");

                var sku = $"{color.Substring(0, Math.Min(3, color.Length)).ToUpper()}-{size.ToUpper()}-{DateTime.Now.Ticks.ToString().Substring(10)}";

                var variant = new ProductVariant
                {
                    ProductId = productId,
                    Color = color,
                    Size = size,
                    Stock = stock,
                    SKU = sku,
                    DiscountPercent = discountPercent, // ✅ Lưu % giảm
                    DiscountPrice = discountPrice       // ✅ Lưu giá giảm
                };

                _context.ProductVariants.Add(variant);
                var saved = await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Saved {saved} records");
                Console.WriteLine($"Variant ID: {variant.Id}");
                Console.WriteLine($"========================\n");

                TempData["SuccessMessage"] = "Thêm biến thể thành công!";
                return RedirectToAction("ManageVariants", new { id = productId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("ManageVariants", new { id = productId });
            }
        }

        // POST: Admin/Products/DeleteVariant
        [HttpPost]
        public async Task<IActionResult> DeleteVariant(int id, int productId)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant != null)
            {
                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa biến thể thành công!";
            }
            return RedirectToAction("ManageVariants", new { id = productId });
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,Stock,CategoryId,HasVariants")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // ✅ KIỂM TRA VÀ HIỂN THỊ CẢNH BÁO
            var activeOrdersCount = await _context.OrderDetails
                .Include(od => od.Order)
                .CountAsync(od => od.ProductId == id &&
                                 od.Order.Status != "Đã giao" &&
                                 od.Order.Status != "Đã hủy");

            ViewBag.HasActiveOrders = activeOrdersCount > 0;
            ViewBag.ActiveOrdersCount = activeOrdersCount;

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm!";
                return RedirectToAction(nameof(Index));
            }

            // ✅ KIỂM TRA SẢN PHẨM CÓ TRONG ĐƠN HÀNG ĐANG XỬ LÝ KHÔNG
            var hasActiveOrders = await _context.OrderDetails
                .Include(od => od.Order)
                .AnyAsync(od => od.ProductId == id &&
                               od.Order.Status != "Đã giao" &&
                               od.Order.Status != "Đã hủy");

            if (hasActiveOrders)
            {
                // ❌ KHÔNG CHO XÓA
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm! Sản phẩm đang có trong đơn hàng chưa hoàn thành (Đang xử lý, Đã xác nhận, Đang giao hàng).";
                return RedirectToAction(nameof(Index));
            }

            // ✅ CHO PHÉP XÓA
            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa sản phẩm: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        // GET: Admin/Products/ManageImages/5
        public async Task<IActionResult> ManageImages(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/UploadImage
        [HttpPost]
        public async Task<IActionResult> UploadProductImage(int productId, IFormFile file, bool isPrimary = false)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh!";
                return RedirectToAction("ManageImages", new { id = productId });
            }

            // Validate file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif, .webp)!";
                return RedirectToAction("ManageImages", new { id = productId });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File không được vượt quá 5MB!";
                return RedirectToAction("ManageImages", new { id = productId });
            }

            try
            {
                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{extension}";

                // Đường dẫn lưu file
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

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

                var imageUrl = $"/images/products/{fileName}";

                // Nếu set làm ảnh chính, bỏ primary của ảnh cũ
                if (isPrimary)
                {
                    var oldPrimary = await _context.ProductImages
                        .Where(i => i.ProductId == productId && i.IsPrimary)
                        .ToListAsync();

                    foreach (var img in oldPrimary)
                    {
                        img.IsPrimary = false;
                    }
                }

                // Lấy display order tiếp theo
                var maxOrder = await _context.ProductImages
                    .Where(i => i.ProductId == productId)
                    .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;

                // Thêm ảnh mới
                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = imageUrl,
                    IsPrimary = isPrimary,
                    DisplayOrder = maxOrder + 1
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();

                // Cập nhật ImageUrl của product nếu là ảnh đầu tiên hoặc ảnh chính
                var product = await _context.Products.FindAsync(productId);
                if (product != null && (string.IsNullOrEmpty(product.ImageUrl) || isPrimary))
                {
                    product.ImageUrl = imageUrl;
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Upload ảnh thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi upload: {ex.Message}";
            }

            return RedirectToAction("ManageImages", new { id = productId });
        }

        // POST: Admin/Products/SetPrimaryImage
        [HttpPost]
        public async Task<IActionResult> SetPrimaryImage(int imageId, int productId)
        {
            // Bỏ primary của tất cả ảnh
            var allImages = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            foreach (var img in allImages)
            {
                img.IsPrimary = false;
            }

            // Set primary cho ảnh được chọn
            var selectedImage = allImages.FirstOrDefault(i => i.Id == imageId);
            if (selectedImage != null)
            {
                selectedImage.IsPrimary = true;

                // Cập nhật ImageUrl của product
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.ImageUrl = selectedImage.ImageUrl;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã đặt làm ảnh chính!";

            return RedirectToAction("ManageImages", new { id = productId });
        }

        // POST: Admin/Products/DeleteImage
        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(int id, int productId)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image != null)
            {
                // Xóa file vật lý
                try
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch { }

                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa ảnh thành công!";
            }

            return RedirectToAction("ManageImages", new { id = productId });
        }

        // POST: Admin/Products/ReorderImages
        [HttpPost]
        public async Task<IActionResult> ReorderImages(int productId, int imageId, string direction)
        {
            var images = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var currentImage = images.FirstOrDefault(i => i.Id == imageId);
            if (currentImage == null)
            {
                return RedirectToAction("ManageImages", new { id = productId });
            }

            var currentIndex = images.IndexOf(currentImage);

            if (direction == "up" && currentIndex > 0)
            {
                var prevImage = images[currentIndex - 1];
                var temp = currentImage.DisplayOrder;
                currentImage.DisplayOrder = prevImage.DisplayOrder;
                prevImage.DisplayOrder = temp;
            }
            else if (direction == "down" && currentIndex < images.Count - 1)
            {
                var nextImage = images[currentIndex + 1];
                var temp = currentImage.DisplayOrder;
                currentImage.DisplayOrder = nextImage.DisplayOrder;
                nextImage.DisplayOrder = temp;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageImages", new { id = productId });
        }
    }
}
