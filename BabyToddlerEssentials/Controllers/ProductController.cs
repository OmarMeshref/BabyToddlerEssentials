using BabyToddlerEssentials.Data;
using BabyToddlerEssentials.Models;
using BabyToddlerEssentials.Models.Enums;
using BabyToddlerEssentials.Services;
using BabyToddlerEssentials.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BabyToddlerEssentials.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(
            ApplicationDbContext context,
            IImageService imageService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
        }

        // =========================================================
        // PUBLIC: Product listing (browse + search + filter + sort + paging)
        // GET /Product  or  /Product/Index
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page = 1)
        {
            const int pageSize = 7;
            if (page < 1) page = 1;

            // Only active products are shown to customers
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductReviews)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Search: partial match on product name OR category name
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    p.Category.Name.Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice ?? p.Price) >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => (p.DiscountPrice ?? p.Price) <= maxPrice.Value);

            // Sorting
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt) // "newest" (default)
            };

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var cards = products.Select(p =>
            {
                var approved = p.ProductReviews
                    .Where(r => r.Status == ModerationStatus.Approved)
                    .ToList();

                return new ProductCardVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    PrimaryImagePath = p.ProductImages
                        .FirstOrDefault(i => i.IsPrimary)?.ImagePath
                        ?? p.ProductImages.FirstOrDefault()?.ImagePath,
                    InStock = p.StockQuantity > 0,
                    ReviewCount = approved.Count,
                    AverageRating = approved.Count > 0 ? approved.Average(r => r.Rating) : 0
                };
            }).ToList();

            var vm = new ProductListVM
            {
                Products = cards,
                Search = search,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            return View(vm);
        }

        // =========================================================
        // PUBLIC: Product details (gallery + approved reviews + avg rating)
        // GET /Product/Details/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound();

            var approved = product.ProductReviews
                .Where(r => r.Status == ModerationStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var vm = new ProductDetailsVM
            {
                Product = product,
                Images = product.ProductImages.ToList(),
                PrimaryImagePath = product.ProductImages
                    .FirstOrDefault(i => i.IsPrimary)?.ImagePath
                    ?? product.ProductImages.FirstOrDefault()?.ImagePath,
                ApprovedReviews = approved,
                ReviewCount = approved.Count,
                AverageRating = approved.Count > 0 ? approved.Average(r => r.Rating) : 0,
                NewReview = new ReviewInputVM { ProductId = product.Id }
            };

            // Whether the current user can leave a review
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _context.Users
                    .Where(u => u.UserName == User.Identity!.Name)
                    .Select(u => u.Id)
                    .FirstOrDefault();

                vm.AlreadyReviewed = product.ProductReviews.Any(r => r.UserId == userId);
                vm.CanReview = !vm.AlreadyReviewed;
            }

            return View(vm);
        }
        // =========================================================
        // ADMIN: Manage products (list for the dashboard)
        // GET /Product/Manage
        // =========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage(string? search, int? categoryId, string? sort, int page = 1)
        {
            const int pageSize = 15;
            if (page < 1) page = 1;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p => p.Name.Contains(term) || p.Category.Name.Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Sorting
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
                "stock_asc" => query.OrderBy(p => p.StockQuantity),
                "stock_desc" => query.OrderByDescending(p => p.StockQuantity),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt) // Default newest
            };

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.Categories = await GetCategoriesAsync();

            return View("/Views/Admin/products/Index.cshtml", products);
        }

        // =========================================================
        // ADMIN: Create product
        // =========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductFormVM
            {
                IsActive = true,
                Categories = await GetCategoriesAsync()
            };
            return View("/Views/Admin/products/Create.cshtml", vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormVM vm)
        {
            // Validate uploaded files are images (before touching the DB)
            ValidateImages(vm.NewImages);

            if (!ModelState.IsValid)
            {
                vm.Categories = await GetCategoriesAsync();
                return View("/Views/Admin/products/Create.cshtml", vm);
            }

            var product = new Product
            {
                Name = vm.Name.Trim(),
                Description = vm.Description,
                Price = vm.Price,
                DiscountPrice = vm.DiscountPrice,
                StockQuantity = vm.StockQuantity,
                Brand = vm.Brand,
                AgeRange = vm.AgeRange,
                IsFeatured = vm.IsFeatured,
                IsActive = vm.IsActive,
                CategoryId = vm.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Save images (first one becomes primary)
            await SaveProductImagesAsync(product.Id, vm.NewImages, isFirstPrimary: true);

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Manage));
        }

        // =========================================================
        // ADMIN: Edit product
        // =========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var vm = new ProductFormVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                Brand = product.Brand,
                AgeRange = product.AgeRange,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                ExistingImages = product.ProductImages.ToList(),
                Categories = await GetCategoriesAsync()
            };

            return View("/Views/Admin/products/Edit.cshtml", vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormVM vm)
        {
            ValidateImages(vm.NewImages);

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == vm.Id);

            if (product == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.ExistingImages = product.ProductImages.ToList();
                vm.Categories = await GetCategoriesAsync();
                return View("/Views/Admin/products/Edit.cshtml", vm);
            }

            product.Name = vm.Name.Trim();
            product.Description = vm.Description;
            product.Price = vm.Price;
            product.DiscountPrice = vm.DiscountPrice;
            product.StockQuantity = vm.StockQuantity;
            product.Brand = vm.Brand;
            product.AgeRange = vm.AgeRange;
            product.IsFeatured = vm.IsFeatured;
            product.IsActive = vm.IsActive;
            product.CategoryId = vm.CategoryId;

            await _context.SaveChangesAsync();

            // Append any newly uploaded images. If the product has no primary yet,
            // the first newly added image becomes primary.
            bool hasPrimary = product.ProductImages.Any(i => i.IsPrimary);
            await SaveProductImagesAsync(product.Id, vm.NewImages, isFirstPrimary: !hasPrimary);

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Manage));
        }

        // =========================================================
        // ADMIN: Delete product
        // =========================================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View("/Views/Admin/products/Delete.cshtml", product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Remove image files from wwwroot first
            foreach (var img in product.ProductImages)
                _imageService.Delete(img.ImagePath);

            _context.Products.Remove(product); // images cascade-delete in the DB
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product deleted.";
            return RedirectToAction(nameof(Manage));
        }

        // =========================================================
        // ADMIN: Delete a single image (helper)
        // POST /Product/DeleteImage
        // =========================================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId);
            if (image == null) return NotFound();

            int productId = image.ProductId;
            bool wasPrimary = image.IsPrimary;

            _imageService.Delete(image.ImagePath);
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // If we removed the primary, promote another image to primary
            if (wasPrimary)
            {
                var next = await _context.ProductImages
                    .Where(i => i.ProductId == productId)
                    .FirstOrDefaultAsync();
                if (next != null)
                {
                    next.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Image removed.";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // =========================================================
        // USER: Submit a review (Pending → admin approves → shown)
        // POST /Product/AddReview
        // =========================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(ReviewInputVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide a valid rating (1–5).";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }

            var productExists = await _context.Products.AnyAsync(p => p.Id == model.ProductId);
            if (!productExists) return NotFound();

            var userId = _userManager.GetUserId(User)!;

            // One review per user per product (matches the unique DB index)
            bool already = await _context.ProductReviews
                .AnyAsync(r => r.ProductId == model.ProductId && r.UserId == userId);

            if (already)
            {
                TempData["ErrorMessage"] = "You have already reviewed this product.";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }

            var review = new ProductReview
            {
                ProductId = model.ProductId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment?.Trim(),
                Status = ModerationStatus.Pending,   // waits for admin approval
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thanks! Your review was submitted and is awaiting approval.";
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }

        // =========================================================
        // Private helpers
        // =========================================================
        private async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        // Adds ModelState errors if any uploaded file isn't a valid image
        private void ValidateImages(List<IFormFile>? files)
        {
            if (files == null) return;
            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                if (!_imageService.IsValidImage(file))
                    ModelState.AddModelError(nameof(ProductFormVM.NewImages),
                        $"'{file.FileName}' is not a valid image (allowed: jpg, png, gif, webp, max 5 MB).");
            }
        }

        // Saves uploaded files to wwwroot and creates ProductImage rows
        private async Task SaveProductImagesAsync(int productId, List<IFormFile>? files, bool isFirstPrimary)
        {
            if (files == null || files.Count == 0) return;

            bool first = isFirstPrimary;
            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var path = await _imageService.SaveAsync(file, "products");

                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImagePath = path,
                    IsPrimary = first
                });

                first = false; // only the first one is primary
            }

            await _context.SaveChangesAsync();
        }
    }
}