using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.ViewModels
{
    // The "submit a testimonial" form
    public class TestimonialInputVM
    {
        [Required(ErrorMessage = "Please write your testimonial.")]
        [MaxLength(1000)]
        [Display(Name = "Your testimonial")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Please choose a rating from 1 to 5.")]
        public int Rating { get; set; }
    }
}