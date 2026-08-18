using BabyToddlerEssentials.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ModerationStatus Status { get; set; }
            = ModerationStatus.Pending;

        // Foreign Keys
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public Product Product { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;
    }
}