using BabyToddlerEssentials.Models;
using BabyToddlerEssentials.Models.Enums;

namespace BabyToddlerEssentials.ViewModels
{
    // The Orders page: split into current vs previous
    public class OrderListVM
    {
        public List<Order> CurrentOrders { get; set; } = new();   // Processing
        public List<Order> PreviousOrders { get; set; } = new();  // Completed / Cancelled
        public bool HasNoOrders => CurrentOrders.Count == 0 && PreviousOrders.Count == 0;
    }

    // Order details + invoice both use the full order (with items + products).
    // A thin wrapper so we can add display helpers without touching the entity.
    public class OrderDetailsVM
    {
        public Order Order { get; set; } = null!;

        public int TotalItems => Order.OrderItems.Sum(i => i.Quantity);
        public bool IsCurrent => Order.Status == OrderStatus.Processing;
    }
}