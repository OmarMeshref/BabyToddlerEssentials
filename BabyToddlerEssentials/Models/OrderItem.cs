using System.ComponentModel.DataAnnotations.Schema;

namespace BabyToddlerEssentials.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }


        // Order

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;


        // Product

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;


        [NotMapped]
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
