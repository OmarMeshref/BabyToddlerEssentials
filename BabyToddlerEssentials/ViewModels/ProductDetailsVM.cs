using BabyToddlerEssentials.Models;

namespace BabyToddlerEssentials.ViewModels
{
    // What the product page (Product/Details) needs
    public class ProductDetailsVM
    {
        public Product Product { get; set; } = null!;
        public List<ProductImage> Images { get; set; } = new();
        public string? PrimaryImagePath { get; set; }

        // Only Approved reviews are shown
        public List<ProductReview> ApprovedReviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Helpers for the view
        public bool InStock => Product.StockQuantity > 0;
        public decimal EffectivePrice => Product.DiscountPrice ?? Product.Price;

        // For the "leave a review" form (bound on submit)
        public ReviewInputVM NewReview { get; set; } = new();

        // UI flags decided in the controller
        public bool CanReview { get; set; }        // logged in AND hasn't reviewed yet
        public bool AlreadyReviewed { get; set; }

        public bool HasPurchased { get; set; }
    }
}