namespace SportsShop.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; } // ✅ Đây phải là giá SAU GIẢM
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }

        // Thêm Size và Color
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int? VariantId { get; set; }

        public decimal TotalPrice => Price * Quantity; // ✅ Tính tổng từ Price (đã giảm)

        // Để hiển thị đầy đủ thông tin
        public string DisplayName => string.IsNullOrEmpty(Size) && string.IsNullOrEmpty(Color)
            ? ProductName
            : $"{ProductName} - {Size} - {Color}";
    }
}