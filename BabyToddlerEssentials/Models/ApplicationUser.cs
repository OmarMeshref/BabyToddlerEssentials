using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        public ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
    }
}
