namespace SportsShop.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; } = false; // Ảnh chính
        public int DisplayOrder { get; set; } = 0; // Thứ tự hiển thị
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public Product? Product { get; set; }
    }
}