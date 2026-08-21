namespace BabyToddlerEssentials.ViewModels.Categories
{
    public class CategoriesIndexViewModel
    {
        public string? SearchTerm { get; set; }

        public List<CategoriesCardViewModel> Categories { get; set; } = [];
    }
}
