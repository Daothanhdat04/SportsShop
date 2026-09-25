using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportsShop.Models;

namespace SportsShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình relationship
            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ THAY ĐỔI Ở ĐÂY
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.SetNull);  // ← Thay đổi này

            // Seed data cho Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Giày thể thao", Description = "Giày chạy bộ, bóng đá, tennis" },
                new Category { Id = 2, Name = "Quần áo", Description = "Áo, quần thể thao" },
                new Category { Id = 3, Name = "Phụ kiện", Description = "Balo, găng tay, băng đô" }
            );

            // Seed data cho Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Giày Nike Air Max", Description = "Giày chạy bộ cao cấp", Price = 2500000, ImageUrl = "/images/nike-air-max.jpg", Stock = 50, CategoryId = 1, HasVariants = false },
                new Product { Id = 2, Name = "Giày Adidas Ultraboost", Description = "Giày chạy bộ êm ái", Price = 3000000, ImageUrl = "/images/adidas-ultraboost.jpg", Stock = 30, CategoryId = 1, HasVariants = false },
                new Product { Id = 3, Name = "Áo thể thao Nam", Description = "Áo thoáng mát", Price = 350000, ImageUrl = "/images/sport-shirt.jpg", Stock = 100, CategoryId = 2, HasVariants = true }
            );


            // Seed data cho ProductVariants
            modelBuilder.Entity<ProductVariant>().HasData(
                new ProductVariant { Id = 1, ProductId = 3, Size = "M", Color = "Đỏ", Stock = 20, SKU = "AO-RED-M" },
                new ProductVariant { Id = 2, ProductId = 3, Size = "L", Color = "Đỏ", Stock = 15, SKU = "AO-RED-L" },
                new ProductVariant { Id = 3, ProductId = 3, Size = "XL", Color = "Đỏ", Stock = 10, SKU = "AO-RED-XL" },
                new ProductVariant { Id = 4, ProductId = 3, Size = "M", Color = "Xanh", Stock = 25, SKU = "AO-BLUE-M" },
                new ProductVariant { Id = 5, ProductId = 3, Size = "L", Color = "Xanh", Stock = 20, SKU = "AO-BLUE-L" },
                new ProductVariant { Id = 6, ProductId = 3, Size = "XL", Color = "Xanh", Stock = 10, SKU = "AO-BLUE-XL" },
                new ProductVariant { Id = 7, ProductId = 3, Size = "M", Color = "Đen", Stock = 30, SKU = "AO-BLACK-M" },
                new ProductVariant { Id = 8, ProductId = 3, Size = "L", Color = "Đen", Stock = 25, SKU = "AO-BLACK-L" },
                new ProductVariant { Id = 9, ProductId = 3, Size = "XL", Color = "Đen", Stock = 15, SKU = "AO-BLACK-XL" }
            );
        }
    }
}