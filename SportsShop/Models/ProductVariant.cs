using System.ComponentModel.DataAnnotations.Schema;

namespace SportsShop.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public int Stock { get; set; }
        public string? SKU { get; set; }

        // THÊM MỚI - GIẢM GIÁ CHO TỪNG BIẾN THỂ
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        public int? DiscountPercent { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // COMPUTED PROPERTY - Giá hiển thị của biến thể
        [NotMapped]
        public decimal DisplayPrice
        {
            get
            {
                // Ưu tiên giá giảm của variant trước
                if (DiscountPrice.HasValue && DiscountPrice.Value > 0)
                {
                    return DiscountPrice.Value;
                }
                // Nếu không có, lấy giá giảm của product
                if (Product != null && Product.IsOnDiscount)
                {
                    return Product.DiscountPrice.Value;
                }
                // Cuối cùng lấy giá gốc
                return Product?.Price ?? 0;
            }
        }

        [NotMapped]
        public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice.Value > 0;
    }
}