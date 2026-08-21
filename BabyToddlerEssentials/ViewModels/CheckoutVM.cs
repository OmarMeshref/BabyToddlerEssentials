using System.ComponentModel.DataAnnotations;
using BabyToddlerEssentials.Services;

namespace BabyToddlerEssentials.ViewModels
{
    // The checkout page: delivery details (prefilled from profile, editable)
    // + a fake payment-method choice + a read-only summary of the cart.
    public class CheckoutVM
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Full name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        [Display(Name = "Shipping address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Please choose a payment method.")]
        [Display(Name = "Payment method")]
        public string PaymentMethod { get; set; } = string.Empty;   // UI-only (not stored)

        // Read-only summary (filled by the controller; not posted back)
        public CartView Cart { get; set; } = new();
    }
}