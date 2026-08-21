using BabyToddlerEssentials.Models;

namespace BabyToddlerEssentials.ViewModels
{
    // What the shop/listing page (Product/Index) needs
    public class ProductListVM
    {
        // Results for the current page
        public List<ProductCardVM> Products { get; set; } = new();

        // Filter/search state (also used to re-fill the form)
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Sort { get; set; }   // "newest", "price_asc", "price_desc", "name"

        // Category dropdown
        public List<Category> Categories { get; set; } = new();

        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 7;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // A single product tile on the listing
    public class ProductCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? PrimaryImagePath { get; set; }
        public bool InStock { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Price actually charged (discount if present)
        public decimal EffectivePrice => DiscountPrice ?? Price;
    }
}