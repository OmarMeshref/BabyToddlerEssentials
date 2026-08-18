using System.ComponentModel.DataAnnotations;
namespace BabyToddlerEssentials.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // User
        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;


        // Product
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;
    }
}
