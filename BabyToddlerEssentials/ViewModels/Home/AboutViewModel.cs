namespace BabyToddlerEssentials.ViewModels.Home
{
    public class AboutViewModel
    {
        public List<HomeTestimonialViewModel> Testimonials { get; set; } = [];

        public int ActiveProductsCount { get; set; }

        public int ActiveCategoriesCount { get; set; }
    }
}
