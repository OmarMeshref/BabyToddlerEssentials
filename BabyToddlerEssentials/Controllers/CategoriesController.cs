using BabyToddlerEssentials.Data;
using BabyToddlerEssentials.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BabyToddlerEssentials.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private static readonly string[] AllowedImageContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private bool ValidateImage(IFormFile? imageFile, string propertyName)
        {
            if (imageFile is null)
            {
                return true;
            }


            if (imageFile.Length == 0)
            {
                ModelState.AddModelError(
                    propertyName,
                    "The selected image is empty.");

                return false;
            }


            if (imageFile.Length > MaxImageSize)
            {
                ModelState.AddModelError(
                    propertyName,
                    "The image must not exceed 5 MB.");

                return false;
            }


            var extension =
                Path.GetExtension(imageFile.FileName)
                    .ToLowerInvariant();


            if (!AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    propertyName,
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

                return false;
            }


            if (!AllowedImageContentTypes.Contains(
                    imageFile.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    propertyName,
                    "The uploaded file must be a valid image.");

                return false;
            }


            return true;
        }

        private async Task<string> SaveCategoryImageAsync(IFormFile imageFile)
        {
            var extension =
                Path.GetExtension(imageFile.FileName)
                    .ToLowerInvariant();


            var fileName =
                $"{Guid.NewGuid():N}{extension}";


            var folderPath =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "categories");


            Directory.CreateDirectory(folderPath);


            var physicalPath =
                Path.Combine(
                    folderPath,
                    fileName);


            await using var stream =
                new FileStream(
                    physicalPath,
                    FileMode.Create);


            await imageFile.CopyToAsync(stream);


            return $"/uploads/categories/{fileName}";
        }

        private void DeleteCategoryImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }


            var relativePath =
                imagePath.TrimStart('/')
                         .Replace(
                             '/',
                             Path.DirectorySeparatorChar);


            var physicalPath =
                Path.Combine(
                    _environment.WebRootPath,
                    relativePath);


            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private const long MaxImageSize = 5 * 1024 * 1024;

        // PUBLIC
        // Browse active categories + partial-name search
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Name.Contains(search));
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .Select(c => new CategoriesCardViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    ProductCount = c.Products.Count(p => p.IsActive)
                })
                .ToListAsync();

            var model = new CategoriesIndexViewModel
            {
                SearchTerm = search,
                Categories = categories
            };

            return View(model);
        }

        // PUBLIC
        // Category details + active products
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategoriesDetailsViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    Products = c.Products
                        .Where(p => p.IsActive)
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => new CategoriesProductViewModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Price = p.Price,
                            StockQuantity = p.StockQuantity,
                            ImagePath = p.ProductImages
                                .OrderByDescending(i => i.IsPrimary)
                                .ThenBy(i => i.Id)
                                .Select(i => i.ImagePath)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (category is null)
            {
                return NotFound();
            }

            return View(category);
        }

        // ADMIN
        // Manage all categories
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(string? search, bool? isActive)
        {
            var query = _context.Categories
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(c => c.Name.Contains(search));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .Select(c => new CategoriesManageItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count()
                })
                .ToListAsync();

            var model = new CategoriesManageViewModel
            {
                SearchTerm = search,
                IsActive = isActive,
                Categories = categories
            };

            return View("/Views/Admin/categories/Index.cshtml", model);
        }

        // ADMIN
        // CREATE GET
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoriesFormViewModel());
        }

        // ADMIN
        // CREATE POST
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriesFormViewModel model)
        {
            model.Name = model.Name.Trim();


            var nameExists =
                await _context.Categories
                    .AnyAsync(c =>
                        c.Name.ToLower() ==
                        model.Name.ToLower());


            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A category with this name already exists.");
            }


            ValidateImage(
                model.ImageFile,
                nameof(model.ImageFile));


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string? imagePath = null;


            if (model.ImageFile is not null)
            {
                imagePath =
                    await SaveCategoryImageAsync(
                        model.ImageFile);
            }


            var category = new Category
            {
                Name = model.Name,

                Description =
                    string.IsNullOrWhiteSpace(model.Description)
                        ? null
                        : model.Description.Trim(),

                ImagePath = imagePath,

                IsActive = true
            };


            _context.Categories.Add(category);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Category created successfully.";


            return RedirectToAction(nameof(Manage));
        }
        // ADMIN
        // EDIT GET
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category =
                await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (category is null)
            {
                return NotFound();
            }


            var model = new CategoriesFormViewModel
            {
                Id = category.Id,

                Name = category.Name,

                Description = category.Description,

                ExistingImagePath = category.ImagePath,

                IsActive = category.IsActive
            };


            return View(model);
        }
        
        // ADMIN
        // EDIT POST
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoriesFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }


            model.Name = model.Name.Trim();


            var duplicateName =
                await _context.Categories
                    .AnyAsync(c =>
                        c.Id != id &&
                        c.Name.ToLower() ==
                        model.Name.ToLower());


            if (duplicateName)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A category with this name already exists.");
            }


            ValidateImage(
                model.ImageFile,
                nameof(model.ImageFile));


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var category =
                await _context.Categories
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);


            if (category is null)
            {
                return NotFound();
            }


            var oldImagePath =
                category.ImagePath;


            string? newImagePath = null;


            if (model.ImageFile is not null)
            {
                newImagePath =
                    await SaveCategoryImageAsync(
                        model.ImageFile);
            }


            category.Name =
                model.Name;


            category.Description =
                string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim();


            category.IsActive =
                model.IsActive;


            if (newImagePath is not null)
            {
                category.ImagePath =
                    newImagePath;
            }


            await _context.SaveChangesAsync();


            if (newImagePath is not null)
            {
                DeleteCategoryImage(
                    oldImagePath);
            }


            TempData["SuccessMessage"] =
                "Category updated successfully.";


            return RedirectToAction(nameof(Manage));
        }
        
        // ADMIN
        // DELETE CONFIRMATION
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoriesManageItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count()
                })
                .FirstOrDefaultAsync();


            if (category is null)
            {
                return NotFound();
            }


            return View(category);
        }

        // ADMIN
        // SOFT DELETE
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);


            if (category is null)
            {
                return NotFound();
            }


            if (!category.IsActive)
            {
                TempData["ErrorMessage"] = "This category is already inactive.";

                return RedirectToAction(nameof(Manage));
            }


            category.IsActive = false;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] = "Category deactivated successfully.";


            return RedirectToAction(nameof(Manage));
        }
    }
}
