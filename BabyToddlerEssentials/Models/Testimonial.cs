using BabyToddlerEssentials.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public ModerationStatus Status { get; set; } = ModerationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Property
        public ApplicationUser User { get; set; } = null!;
    }
}