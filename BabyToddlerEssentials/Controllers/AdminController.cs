using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BabyToddlerEssentials.Data;
using BabyToddlerEssentials.Models;
using BabyToddlerEssentials.Models.Enums; 

namespace BabyToddlerEssentials.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= 1. Dashboard Statistics =================
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            return View();
        }

        // ================= 2. Manage Users =================
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // ================= 3. Manage Orders =================
        public async Task<IActionResult> Orders(string? status)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status.ToString() == status);
            }

            var orders = await ordersQuery.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View("/Views/Admin/Orders/Index.cshtml", orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View("/Views/Admin/Orders/OrderDetails.cshtml", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order status updated successfully!";
            return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        }

        // ================= 4. Manage Reviews & Ratings =================

        public async Task<IActionResult> Reviews(ModerationStatus? status)
        {
            
            ViewBag.PendingCount = await _context.ProductReviews.CountAsync(r => r.Status == ModerationStatus.Pending);
            ViewBag.ApprovedCount = await _context.ProductReviews.CountAsync(r => r.Status == ModerationStatus.Approved);
            ViewBag.RejectedCount = await _context.ProductReviews.CountAsync(r => r.Status == ModerationStatus.Rejected);
            ViewBag.CurrentStatus = status;

            var reviewsQuery = _context.ProductReviews
                .Include(r => r.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(r => r.User)
                .AsQueryable();

            if (status.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.Status == status.Value);
            }
            else
            {
                
                reviewsQuery = reviewsQuery.Where(r => r.Status == ModerationStatus.Pending);
            }

            var reviews = await reviewsQuery.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(reviews);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.Status = ModerationStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Review approved successfully!";
            }
            return RedirectToAction(nameof(Reviews));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReview(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                review.Status = ModerationStatus.Rejected;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Review rejected successfully!";
            }
            return RedirectToAction(nameof(Reviews));
        }

        // ================= 5. Manage Testimonials =================
       
        public async Task<IActionResult> Testimonials(ModerationStatus? status)
        {
            ViewBag.PendingCount = await _context.Testimonials.CountAsync(t => t.Status == ModerationStatus.Pending);
            ViewBag.ApprovedCount = await _context.Testimonials.CountAsync(t => t.Status == ModerationStatus.Approved);
            ViewBag.RejectedCount = await _context.Testimonials.CountAsync(t => t.Status == ModerationStatus.Rejected);
            ViewBag.CurrentStatus = status;

            var testimonialsQuery = _context.Testimonials
                .Include(t => t.User)
                .AsQueryable();

            if (status.HasValue)
            {
                testimonialsQuery = testimonialsQuery.Where(t => t.Status == status.Value);
            }
            else
            {
                
                testimonialsQuery = testimonialsQuery.Where(t => t.Status == ModerationStatus.Pending);
            }

            var testimonials = await testimonialsQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return View(testimonials);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTestimonial(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                testimonial.Status = ModerationStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Testimonial approved successfully!";
            }
            return RedirectToAction(nameof(Testimonials));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTestimonial(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                testimonial.Status = ModerationStatus.Rejected;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Testimonial rejected successfully!";
            }
            return RedirectToAction(nameof(Testimonials));
        }
    }
}
