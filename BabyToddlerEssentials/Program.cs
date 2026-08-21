using BabyToddlerEssentials.Data;
using BabyToddlerEssentials.Models;
using BabyToddlerEssentials.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// Database
// =========================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// =========================================
// Identity
// =========================================

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/User/AccessDenied";
});


// Dummy Email Sender for development
builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();


// =========================================
// MVC
// =========================================

builder.Services.AddControllersWithViews();

// =========================================
// Application Services
// =========================================
// ImageService: saves product image uploads to wwwroot/images/products and
// returns the path we store in ProductImage.ImagePath (validates image type + size).

builder.Services.AddScoped<IImageService, BabyToddlerEssentials.Services.ImageService>();


// =========================================
// Cart Service
// =========================================
// CartService: manages the shopping cart, which lives in the user's SESSION
// Stores only ProductId + Quantity; prices/stock are
// always read live from the database. Enforces "one line per product"
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, BabyToddlerEssentials.Services.CartService>();

// =========================================
// Session
// =========================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();


// =========================================
// HTTP Pipeline
// =========================================

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();


// =========================================
// Seed Roles + Admin
// =========================================

await DbInitializer.InitializeAsync(
    app.Services,
    app.Configuration);

app.Run();