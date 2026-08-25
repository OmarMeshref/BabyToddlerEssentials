namespace BabyToddlerEssentials.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<HomeCategoryViewModel> Categories { get; set; } = [];
        public List<HomeProductViewModel> Products { get; set; } = [];
        public List<HomeTestimonialViewModel> Testimonials { get; set; } = [];

        public int ActiveProductsCount { get; set; }
        public int ActiveCategoriesCount { get; set; }
    }

    public class HomeCategoryViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public string? Description { get; set; }

        public int ProductCount { get; set; }
    }

    public class HomeProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public bool IsInWishlist { get; set; }
    }

    public class HomeTestimonialViewModel
    {
        public string CustomerName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(CustomerName) ? "?" : CustomerName.Trim().Substring(0, 1).ToUpper();
    }
}
