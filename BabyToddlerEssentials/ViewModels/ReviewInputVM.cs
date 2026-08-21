using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.ViewModels
{
    // The add-review form (posted to Product/AddReview)
    public class ReviewInputVM
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Please choose a rating from 1 to 5.")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Your review")]
        public string? Comment { get; set; }
    }
}