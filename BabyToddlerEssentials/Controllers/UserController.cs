using BabyToddlerEssentials.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BabyToddlerEssentials.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

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
    }
}