using BabyToddlerEssentials.Data;
using BabyToddlerEssentials.Models;
using BabyToddlerEssentials.Models.Enums;
using BabyToddlerEssentials.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BabyToddlerEssentials.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Take(6)
                .Select(c => new HomeCategoryViewModel {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive)
                })
                .ToListAsync();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Select(p => new HomeProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,

                    CategoryName = _context.Categories
                        .Where(c => c.Id == p.CategoryId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? string.Empty,

                    ImagePath = _context.ProductImages
                        .Where(i => i.ProductId == p.Id)
                        .OrderByDescending(i => i.IsPrimary)
                        .ThenBy(i => i.Id)
                        .Select(i => i.ImagePath)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var testimonials = await (
                from testimonial in _context.Testimonials.AsNoTracking()
                join user in _context.Users.AsNoTracking()
                    on testimonial.UserId equals user.Id
                where testimonial.Status == ModerationStatus.Approved
                orderby testimonial.CreatedAt descending
                select new HomeTestimonialViewModel
                {
                    CustomerName = string.IsNullOrWhiteSpace(user.FullName) ? "Customer" : user.FullName,
                    Message = testimonial.Message,
                    CreatedAt = testimonial.CreatedAt
                })
                .Take(6)
                .ToListAsync();

            var model = new HomeIndexViewModel
            {
                Categories = categories,
                Products = products,
                Testimonials = testimonials,

                ActiveProductsCount = await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.IsActive),

                ActiveCategoriesCount = await _context.Categories
                .AsNoTracking()
                .CountAsync(c => c.IsActive)
            };

            return View(model);

        }

        public async Task<IActionResult> About()
        {
            var testimonials = await (
                from testimonial in _context.Testimonials.AsNoTracking()
                join user in _context.Users.AsNoTracking()
                    on testimonial.UserId equals user.Id
                where testimonial.Status == ModerationStatus.Approved
                orderby testimonial.CreatedAt descending
                select new HomeTestimonialViewModel
                {
                    CustomerName = string.IsNullOrWhiteSpace(user.FullName) ? "Customer" : user.FullName,
                    Message = testimonial.Message,
                    CreatedAt = testimonial.CreatedAt
                })
                .Take(9)
                .ToListAsync();

            var model = new AboutViewModel
            {
                Testimonials = testimonials,

                ActiveProductsCount = await _context.Products
                    .AsNoTracking()
                    .CountAsync(p => p.IsActive),

                ActiveCategoriesCount = await _context.Categories
                    .AsNoTracking()
                    .CountAsync(c => c.IsActive)
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

    }
}
