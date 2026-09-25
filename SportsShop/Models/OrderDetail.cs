namespace SportsShop.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }  // ✅ Thêm dấu ? để cho phép null
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public string ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? VariantInfo { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}