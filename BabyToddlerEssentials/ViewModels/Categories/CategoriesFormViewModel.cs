using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.ViewModels.Categories
{
    public class CategoriesFormViewModel
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;


        [MaxLength(500)]
        public string? Description { get; set; }


        [Display(Name = "Category Image")]
        public IFormFile? ImageFile { get; set; }


        // Used in Edit to display/keep current image.
        public string? ExistingImagePath { get; set; }


        public bool IsActive { get; set; } = true;
    }
}
