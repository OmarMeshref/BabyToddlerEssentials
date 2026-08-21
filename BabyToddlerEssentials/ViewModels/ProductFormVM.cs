using BabyToddlerEssentials.Models;
using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.ViewModels
{
    // Admin create/edit product form
    public class ProductFormVM
    {
        public int Id { get; set; }   // 0 = create, >0 = edit

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "999999.99")]
        public decimal Price { get; set; }

        [Range(typeof(decimal), "0.01", "999999.99")]
        [Display(Name = "Discount price")]
        public decimal? DiscountPrice { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Stock quantity")]
        public int StockQuantity { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        [Display(Name = "Age range")]
        public string? AgeRange { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // Category dropdown source
        public List<Category> Categories { get; set; } = new();

        // New images being uploaded (validated as images in the controller)
        [Display(Name = "Product images")]
        public List<IFormFile>? NewImages { get; set; }

        // Existing images (edit mode) — so the view can show/remove them
        public List<ProductImage> ExistingImages { get; set; } = new();
    }
}