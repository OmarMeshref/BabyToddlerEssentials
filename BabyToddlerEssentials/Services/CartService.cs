using System.Text.Json;
using BabyToddlerEssentials.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyToddlerEssentials.Services
{
    // One line stored in the session cart. We deliberately store ONLY the
    // ProductId + Quantity — never the price. Prices are always read live
    // from the database so the cart can never show a stale/incorrect price.
    public class CartLine
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public interface ICartService
    {
        // Raw session contents (ProductId + Quantity only)
        List<CartLine> GetLines();

        // Add a product; if it's already in the cart, increase its quantity
        // instead of adding a second line. Caps at the product's StockQuantity.
        // Returns a small result so the controller can show the right toast.
        Task<CartActionResult> AddAsync(int productId, int quantity = 1);

        // Set an exact quantity for a product (from the cart page). Caps at stock.
        // Quantity <= 0 removes the line.
        Task<CartActionResult> UpdateQuantityAsync(int productId, int quantity);

        void Remove(int productId);
        void Clear();

        // Total number of items (sum of quantities) — handy for a navbar badge.
        int GetCount();

        // Builds the full cart for display: joins session lines with live product
        // data (name, price, image, stock) and computes subtotals + total.
        Task<CartView> GetCartAsync();
    }

    // Result of an add/update, so the controller knows what message to show.
    public class CartActionResult
    {
        public bool Success { get; set; }
        public bool Capped { get; set; }          // quantity was reduced to available stock
        public int AppliedQuantity { get; set; }  // the quantity actually set
        public string? Message { get; set; }
    }

    // Display models for the cart page (built fresh from the DB each time).
    public class CartView
    {
        public List<CartItemView> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int ItemCount => Items.Sum(i => i.Quantity);
        public bool IsEmpty => Items.Count == 0;
    }

    public class CartItemView
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }      // effective price (discount if any)
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public string? ImagePath { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class CartService : ICartService
    {
        private const string SessionKey = "CART";

        private readonly IHttpContextAccessor _http;
        private readonly ApplicationDbContext _context;

        public CartService(IHttpContextAccessor http, ApplicationDbContext context)
        {
            _http = http;
            _context = context;
        }

        private ISession Session => _http.HttpContext!.Session;

        public List<CartLine> GetLines()
        {
            var json = Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartLine>();

            return JsonSerializer.Deserialize<List<CartLine>>(json) ?? new List<CartLine>();
        }

        private void Save(List<CartLine> lines)
        {
            Session.SetString(SessionKey, JsonSerializer.Serialize(lines));
        }

        public async Task<CartActionResult> AddAsync(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (product == null)
                return new CartActionResult { Success = false, Message = "Product not found." };

            if (product.StockQuantity <= 0)
                return new CartActionResult { Success = false, Message = "This product is out of stock." };

            var lines = GetLines();
            var line = lines.FirstOrDefault(l => l.ProductId == productId);

            // Merge: same product => increase quantity, no duplicate line.
            int current = line?.Quantity ?? 0;
            int desired = current + quantity;

            // Cap at available stock.
            bool capped = false;
            if (desired > product.StockQuantity)
            {
                desired = product.StockQuantity;
                capped = true;
            }

            if (line == null)
                lines.Add(new CartLine { ProductId = productId, Quantity = desired });
            else
                line.Quantity = desired;

            Save(lines);

            return new CartActionResult
            {
                Success = true,
                Capped = capped,
                AppliedQuantity = desired,
                Message = capped
                    ? $"Only {product.StockQuantity} in stock — quantity set to {desired}."
                    : "Added to cart."
            };
        }

        public async Task<CartActionResult> UpdateQuantityAsync(int productId, int quantity)
        {
            var lines = GetLines();
            var line = lines.FirstOrDefault(l => l.ProductId == productId);
            if (line == null)
                return new CartActionResult { Success = false, Message = "Item is not in your cart." };

            if (quantity <= 0)
            {
                lines.Remove(line);
                Save(lines);
                return new CartActionResult { Success = true, AppliedQuantity = 0, Message = "Item removed." };
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (product == null)
            {
                lines.Remove(line);
                Save(lines);
                return new CartActionResult { Success = false, Message = "Product no longer available; removed from cart." };
            }

            bool capped = false;
            if (quantity > product.StockQuantity)
            {
                quantity = Math.Max(product.StockQuantity, 1);
                capped = true;
            }

            line.Quantity = quantity;
            Save(lines);

            return new CartActionResult
            {
                Success = true,
                Capped = capped,
                AppliedQuantity = quantity,
                Message = capped ? $"Only {product.StockQuantity} in stock — quantity set to {quantity}." : "Cart updated."
            };
        }

        public void Remove(int productId)
        {
            var lines = GetLines();
            var line = lines.FirstOrDefault(l => l.ProductId == productId);
            if (line != null)
            {
                lines.Remove(line);
                Save(lines);
            }
        }

        public void Clear() => Session.Remove(SessionKey);

        public int GetCount() => GetLines().Sum(l => l.Quantity);

        public async Task<CartView> GetCartAsync()
        {
            var lines = GetLines();
            var view = new CartView();

            if (lines.Count == 0)
                return view;

            var ids = lines.Select(l => l.ProductId).ToList();

            var products = await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            foreach (var line in lines)
            {
                var product = products.FirstOrDefault(p => p.Id == line.ProductId);
                if (product == null) continue; // product deleted since it was added

                view.Items.Add(new CartItemView
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    UnitPrice = product.DiscountPrice ?? product.Price,
                    Quantity = line.Quantity,
                    StockQuantity = product.StockQuantity,
                    ImagePath = product.ProductImages.FirstOrDefault(i => i.IsPrimary)?.ImagePath
                                ?? product.ProductImages.FirstOrDefault()?.ImagePath
                });
            }

            return view;
        }
    }
}