namespace SportsShop.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        // Navigation property
        public ICollection<Product>? Products { get; set; }
    }
}