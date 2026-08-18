using System.ComponentModel.DataAnnotations;

namespace BabyToddlerEssentials.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;


        // Foreign Key
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;
    }
}
