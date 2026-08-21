namespace BabyToddlerEssentials.ViewModels.Categories
{
    public class CategoriesManageViewModel
    {
        public string? SearchTerm { get; set; }

        public bool? IsActive { get; set; }

        public List<CategoriesManageItemViewModel> Categories { get; set; } = [];
    }

    public class CategoriesManageItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int ProductCount { get; set; }
    }
}
