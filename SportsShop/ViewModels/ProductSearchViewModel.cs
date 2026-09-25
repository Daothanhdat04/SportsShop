using SportsShop.Models;
using X.PagedList;

namespace SportsShop.ViewModels
{
    public class ProductSearchViewModel
    {
        public IPagedList<Product>? Products { get; set; }
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortOrder { get; set; }
        public List<Category>? Categories { get; set; }
    }
}