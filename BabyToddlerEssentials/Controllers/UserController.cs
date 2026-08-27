using BabyToddlerEssentials.Data;
using BabyToddlerEssentials.Models.Enums;
using BabyToddlerEssentials.Services;
using BabyToddlerEssentials.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyToddlerEssentials.Controllers
{
    // NOTE: [Authorize] is intentionally NOT on the class — the cart must work
    // for guests (no login). Each action that needs a login has its own [Authorize].
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;
        private const string PendingKey = "PENDING_CHECKOUT";
        public UserController(
            UserManager<ApplicationUser> userManager,
            ICartService cartService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _cartService = cartService;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new ProfileViewModel
            {
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? "Not provided" : user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // =========================================================
        // POST /User/ToggleWishlist — add if not saved, remove if saved
        // Used by the heart button on product cards (AJAX)
        // =========================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var userId = _userManager.GetUserId(User)!;

            var existing = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            bool nowInWishlist;

            if (existing != null)
            {
                _context.WishlistItems.Remove(existing);
                nowInWishlist = false;
            }
            else
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

                if (product == null)
                    return Json(new { success = false, message = "Product not found." });

                _context.WishlistItems.Add(new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                });
                nowInWishlist = true;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                inWishlist = nowInWishlist,
                message = nowInWishlist ? "Added to your wishlist." : "Removed from your wishlist."
            });
        }

        // =========================================================
        // CART  (no login required — cart lives in session)
        // =========================================================

        // GET /User/Cart  — show the cart with live prices + total
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var cart = await _cartService.GetCartAsync();
            return View(cart);
        }

        // POST /User/AddToCart — add a product (merges + caps at stock)
        // Behavior for normal form posts is UNCHANGED (redirect + TempData).
        // AJAX callers (fetch/XHR with X-Requested-With header) get JSON instead.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            var result = await _cartService.AddAsync(productId, quantity);
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            // AJAX call → return JSON instead of redirecting
            if (isAjax)
            {
                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    capped = result.Capped,
                    cartCount = _cartService.GetCount()
                });
            }

            if (!result.Success)
                TempData["ErrorMessage"] = result.Message;
            else if (result.Capped)
                TempData["InfoMessage"] = result.Message;
            else
                TempData["SuccessMessage"] = result.Message;

            // Go back where the user came from (product page / listing), else the cart
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Cart));
        }

        // POST /User/UpdateCartQuantity — set an exact quantity (from the cart page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartQuantity(int productId, int quantity)
        {
            var result = await _cartService.UpdateQuantityAsync(productId, quantity);

            if (!result.Success)
                TempData["ErrorMessage"] = result.Message;
            else if (result.Capped)
                TempData["InfoMessage"] = result.Message;
            else
                TempData["SuccessMessage"] = result.Message;

            return RedirectToAction(nameof(Cart));
        }

        // POST /User/RemoveFromCart — remove a line entirely
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.Remove(productId);
            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToAction(nameof(Cart));
        }

        // POST /User/ClearCart — empty the whole cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            _cartService.Clear();
            TempData["InfoMessage"] = "Your cart is now empty.";
            return RedirectToAction(nameof(Cart));
        }

        // =========================================================
        // WISHLIST  (login required)
        // =========================================================

        // GET /User/Wishlist — the current user's saved products
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Wishlist()
        {
            var userId = _userManager.GetUserId(User)!;

            var items = await _context.WishlistItems
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductImages)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        // POST /User/AddToWishlist — save a product (one per user+product)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int productId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User)!;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (product == null)
            {
                if (isAjax) return Json(new { success = false, message = "Product not found." });
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectBackOrWishlist(returnUrl);
            }

            bool exists = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            if (exists)
            {
                if (isAjax) return Json(new { success = true, info = true, message = "This product is already in your wishlist." });
                TempData["InfoMessage"] = "This product is already in your wishlist.";
                return RedirectBackOrWishlist(returnUrl);
            }

            _context.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            if (isAjax) return Json(new { success = true, message = "Added to your wishlist." });

            TempData["SuccessMessage"] = "Added to your wishlist.";
            return RedirectBackOrWishlist(returnUrl);
        }

        // POST /User/RemoveFromWishlist — remove a saved product
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = _userManager.GetUserId(User)!;

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Removed from your wishlist.";
            }

            return RedirectToAction(nameof(Wishlist));
        }

        // Helper: return to the page the user came from, else the wishlist page.
        private IActionResult RedirectBackOrWishlist(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Wishlist));
        }

        // =========================================================
        // TESTIMONIALS
        //   - Reviews page: PUBLIC, shows APPROVED testimonials
        //   - Submit: login required, one per user, saved as Pending
        // =========================================================

        // GET /User/Reviews — public page listing approved testimonials
        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var approved = await _context.Testimonials
                .Include(t => t.User)
                .Where(t => t.Status == ModerationStatus.Approved)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Tell a logged-in user whether they've already submitted one,
            // so the view can show the form or a "pending/submitted" note.
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User)!;
                ViewBag.AlreadySubmitted = await _context.Testimonials
                    .AnyAsync(t => t.UserId == userId);
            }
            else
            {
                ViewBag.AlreadySubmitted = false;
            }

            return View(approved);
        }

        // GET /User/SubmitTestimonial — the submit form (login required)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SubmitTestimonial()
        {
            var userId = _userManager.GetUserId(User)!;

            // One testimonial per user (enforced in code — no DB constraint).
            bool already = await _context.Testimonials.AnyAsync(t => t.UserId == userId);
            if (already)
            {
                TempData["InfoMessage"] = "You have already submitted a testimonial.";
                return RedirectToAction(nameof(Reviews));
            }

            return View(new TestimonialInputVM());
        }

        // POST /User/SubmitTestimonial — save as Pending (awaits admin approval)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTestimonial(TestimonialInputVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User)!;

            // Guard again on submit (in case they opened two tabs).
            bool already = await _context.Testimonials.AnyAsync(t => t.UserId == userId);
            if (already)
            {
                TempData["InfoMessage"] = "You have already submitted a testimonial.";
                return RedirectToAction(nameof(Reviews));
            }

            _context.Testimonials.Add(new Testimonial
            {
                UserId = userId,
                Message = model.Message.Trim(),
                Rating = model.Rating,
                Status = ModerationStatus.Pending,   // waits for admin approval
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thanks! Your testimonial was submitted and is awaiting approval.";
            return RedirectToAction(nameof(Reviews));
        }

        // =========================================================
        // ORDERS  (login required — users see only their own orders)
        // =========================================================

        // GET /User/Orders — current (Processing) + previous (Completed/Cancelled)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var userId = _userManager.GetUserId(User)!;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(o => o.UserId == userId)
                 .OrderByDescending(o => o.OrderDate)
                 .ToListAsync();

            var vm = new OrderListVM
            {
                CurrentOrders = orders
                    .Where(o => o.Status == OrderStatus.Processing)
                    .ToList(),
                PreviousOrders = orders
                    .Where(o => o.Status == OrderStatus.Completed
                             || o.Status == OrderStatus.Cancelled)
                    .ToList()
            };

            return View(vm);
        }

        // GET /User/OrderDetails/5 — full details of one of the user's orders
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await LoadOwnedOrderAsync(id);
            if (order == null) return NotFound();

            return View(new OrderDetailsVM { Order = order });
        }

        // GET /User/Invoice/5 — printable invoice for one of the user's orders
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await LoadOwnedOrderAsync(id);
            if (order == null) return NotFound();

            return View(new OrderDetailsVM { Order = order });
        }

        // Loads an order with its items + products, but ONLY if it belongs to
        // the current user. Returns null otherwise (so we never leak someone
        // else's order via a guessed id).
        private async Task<Order?> LoadOwnedOrderAsync(int orderId)
        {
            var userId = _userManager.GetUserId(User)!;

            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        // =========================================================
        // CHECKOUT  (login required)
        //   GET  /User/Checkout  — show form (prefilled) + cart summary
        // =========================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = await _cartService.GetCartAsync();

            // Empty cart → back to the cart page with a message
            if (cart.IsEmpty)
            {
                TempData["InfoMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            // Early stock guard (authoritative re-check happens at payment)
            var shortItem = cart.Items.FirstOrDefault(i => i.Quantity > i.StockQuantity);
            if (shortItem != null)
            {
                TempData["ErrorMessage"] =
                    $"'{shortItem.Name}' only has {shortItem.StockQuantity} left. Please update your cart.";
                return RedirectToAction(nameof(Cart));
            }

            // Prefill delivery details from the user's profile (editable)
            var user = await _userManager.GetUserAsync(User);

            var vm = new CheckoutVM
            {
                CustomerName = user?.FullName ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                ShippingAddress = user?.Address ?? string.Empty,
                City = user?.City ?? string.Empty,
                Cart = cart
            };

            return View(vm);
        }

        // =========================================================
        // CHECKOUT — step 2: submit delivery details, then "go to gateway"
        //   POST /User/PlaceOrder
        // This mimics a real external gateway: we validate, stash the order
        // details, then REDIRECT to the (simulated) payment provider.
        // Nothing is saved to the DB yet and stock is NOT touched here.
        // =========================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            var cart = await _cartService.GetCartAsync();

            if (cart.IsEmpty)
            {
                TempData["InfoMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            if (!ModelState.IsValid)
            {
                model.Cart = cart;               // re-fill summary and redisplay
                return View(nameof(Checkout), model);
            }

            // Stash the delivery details in session so the gateway callback
            // can build the order after "payment" succeeds.
            var pending = new PendingCheckout
            {
                CustomerName = model.CustomerName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                ShippingAddress = model.ShippingAddress.Trim(),
                City = model.City.Trim(),
                Notes = model.Notes?.Trim(),
                PaymentMethod = model.PaymentMethod
            };
            HttpContext.Session.SetString(PendingKey,
                System.Text.Json.JsonSerializer.Serialize(pending));

            // Redirect to the simulated external payment provider.
            return RedirectToAction(nameof(PaymentGateway));
        }

        // =========================================================
        // SIMULATED PAYMENT GATEWAY (stands in for an external provider)
        //   GET /User/PaymentGateway — the provider's "pay now" screen
        // In a real integration this would be the provider's hosted page.
        // =========================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PaymentGateway()
        {
            // Must have a pending checkout and a non-empty cart to be here
            if (HttpContext.Session.GetString(PendingKey) == null)
                return RedirectToAction(nameof(Cart));

            var cart = await _cartService.GetCartAsync();
            if (cart.IsEmpty)
                return RedirectToAction(nameof(Cart));

            return View(cart);   // shows the amount + "Pay" / "Cancel" buttons
        }

        // =========================================================
        // GATEWAY CALLBACK — the provider redirects back here with a result.
        //   POST /User/PaymentCallback
        // This is where the order is actually created. Everything happens in
        // ONE transaction: re-check stock, create order + items, decrement
        // stock, mark paid, clear cart. If anything fails, nothing is saved.
        // =========================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentCallback(bool success)
        {
            var pendingJson = HttpContext.Session.GetString(PendingKey);
            if (pendingJson == null)
                return RedirectToAction(nameof(Cart));

            // User cancelled at the gateway
            if (!success)
            {
                HttpContext.Session.Remove(PendingKey);
                TempData["ErrorMessage"] = "Payment was cancelled. Your cart is still saved.";
                return RedirectToAction(nameof(Cart));
            }

            var pending = System.Text.Json.JsonSerializer
                .Deserialize<PendingCheckout>(pendingJson)!;

            var userId = _userManager.GetUserId(User)!;
            var lines = _cartService.GetLines();

            if (lines.Count == 0)
            {
                HttpContext.Session.Remove(PendingKey);
                return RedirectToAction(nameof(Cart));
            }

            // Load the actual products fresh from the DB (authoritative prices + stock)
            var ids = lines.Select(l => l.ProductId).ToList();
            var products = await _context.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            // Everything below is atomic: either the whole order commits, or none of it.
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Processing,
                    IsPaid = true,                       // simulated payment succeeded
                    CustomerName = pending.CustomerName,
                    PhoneNumber = pending.PhoneNumber,
                    ShippingAddress = pending.ShippingAddress,
                    City = pending.City,
                    Notes = pending.Notes,
                    OrderItems = new List<OrderItem>()
                };

                decimal total = 0m;

                foreach (var line in lines)
                {
                    var product = products.FirstOrDefault(p => p.Id == line.ProductId);

                    // Product vanished or went inactive since it was added
                    if (product == null || !product.IsActive)
                    {
                        await tx.RollbackAsync();
                        HttpContext.Session.Remove(PendingKey);
                        TempData["ErrorMessage"] =
                            "One of your items is no longer available. Please review your cart.";
                        return RedirectToAction(nameof(Cart));
                    }

                    // AUTHORITATIVE stock re-check at the moment of purchase
                    if (line.Quantity > product.StockQuantity)
                    {
                        await tx.RollbackAsync();
                        HttpContext.Session.Remove(PendingKey);
                        TempData["ErrorMessage"] =
                            $"'{product.Name}' only has {product.StockQuantity} left. Please update your cart.";
                        return RedirectToAction(nameof(Cart));
                    }

                    var unitPrice = product.DiscountPrice ?? product.Price;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = line.Quantity,
                        UnitPrice = unitPrice            // frozen at purchase time
                    });

                    total += unitPrice * line.Quantity;

                    // Decrement stock now — only because payment succeeded
                    product.StockQuantity -= line.Quantity;
                }

                order.TotalAmount = total;               // computed server-side

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();       // saves order + items + stock changes
                await tx.CommitAsync();

                // Success: clear cart + pending, toast, go to the invoice
                _cartService.Clear();
                HttpContext.Session.Remove(PendingKey);

                TempData["SuccessMessage"] = "Payment successful! Your order has been placed.";
                return RedirectToAction(nameof(Invoice), new { id = order.Id });
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["ErrorMessage"] = "Something went wrong placing your order. Please try again.";
                return RedirectToAction(nameof(Cart));
            }
        }
    }
    // Delivery details held in session between "Place order" and the gateway callback.
    public class PendingCheckout
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
