using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;


namespace SportsShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // THÊM MỚI - GIẢM GIÁ
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; } // Giá sau khi giảm

        public int? DiscountPercent { get; set; } // % giảm giá

        public DateTime? DiscountStartDate { get; set; } // Ngày bắt đầu giảm

        public DateTime? DiscountEndDate { get; set; } // Ngày kết thúc giảm

        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public bool HasVariants { get; set; } = false;

        // Navigation properties
        public Category? Category { get; set; }
        public ICollection<ProductVariant>? Variants { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public ICollection<ProductImage>? Images { get; set; }
        public ICollection<ProductReview>? Reviews { get; set; }


        // COMPUTED PROPERTY - Giá hiển thị
        [NotMapped]
        public decimal DisplayPrice
        {
            get
            {
                // Nếu có giảm giá và đang trong thời gian giảm
                if (DiscountPrice.HasValue && IsOnDiscount)
                {
                    return DiscountPrice.Value;
                }
                return Price;
            }
        }

        // COMPUTED PROPERTY - Đang giảm giá không?
        [NotMapped]
        public bool IsOnDiscount
        {
            get
            {
                var now = DateTime.Now;
                return DiscountPrice.HasValue &&
                       (!DiscountStartDate.HasValue || DiscountStartDate.Value <= now) &&
                       (!DiscountEndDate.HasValue || DiscountEndDate.Value >= now);
            }
        }
        // THÊM MỚI - Lấy ảnh chính
        [NotMapped]
        public string PrimaryImageUrl
        {
            get
            {
                var primaryImage = Images?.FirstOrDefault(i => i.IsPrimary);
                if (primaryImage != null)
                {
                    return primaryImage.ImageUrl;
                }

                var firstImage = Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault();
                if (firstImage != null)
                {
                    return firstImage.ImageUrl;
                }

                return ImageUrl ?? "/images/no-image.jpg";
            }
        }

        // Computed properties cho rating
        [NotMapped]
        public double AverageRating
        {
            get
            {
                if (Reviews == null || !Reviews.Any(r => r.IsApproved))
                    return 0;

                return Reviews.Where(r => r.IsApproved).Average(r => r.Rating);
            }
        }

        [NotMapped]
        public int TotalReviews
        {
            get
            {
                if (Reviews == null)
                    return 0;

                return Reviews.Count(r => r.IsApproved);
            }
        }

        // Đếm số review theo sao
        [NotMapped]
        public Dictionary<int, int> RatingCounts
        {
            get
            {
                var counts = new Dictionary<int, int>
        {
            {5, 0}, {4, 0}, {3, 0}, {2, 0}, {1, 0}
        };

                if (Reviews == null)
                    return counts;

                var approvedReviews = Reviews.Where(r => r.IsApproved).ToList();

                for (int i = 1; i <= 5; i++)
                {
                    counts[i] = approvedReviews.Count(r => r.Rating == i);
                }

                return counts;
            }
        }
    }
}