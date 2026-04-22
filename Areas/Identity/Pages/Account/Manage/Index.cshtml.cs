using System.ComponentModel.DataAnnotations;
using ASP_PROJECT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASP_PROJECT.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(80)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string? OldPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Input.OldPassword = string.IsNullOrWhiteSpace(Input.OldPassword) ? null : Input.OldPassword.Trim();
        Input.NewPassword = string.IsNullOrWhiteSpace(Input.NewPassword) ? null : Input.NewPassword.Trim();
        Input.ConfirmPassword = string.IsNullOrWhiteSpace(Input.ConfirmPassword) ? null : Input.ConfirmPassword.Trim();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var normalizedFullName = Input.FullName.Trim();
        if (!string.Equals(user.FullName, normalizedFullName, StringComparison.Ordinal))
        {
            user.FullName = normalizedFullName;
        }

        var wantsPasswordChange =
            Input.OldPassword is not null ||
            Input.NewPassword is not null ||
            Input.ConfirmPassword is not null;

        if (wantsPasswordChange)
        {
            if (string.IsNullOrWhiteSpace(Input.OldPassword))
            {
                ModelState.AddModelError("Input.OldPassword", "Current password is required to change your password.");
            }

            if (string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                ModelState.AddModelError("Input.NewPassword", "New password is required.");
            }

            if (string.IsNullOrWhiteSpace(Input.ConfirmPassword))
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Please confirm your new password.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var currentPassword = Input.OldPassword!;
            var newPassword = Input.NewPassword!;
            var passwordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = wantsPasswordChange
            ? "Your account details and password were updated."
            : "Your account details were updated.";
        return RedirectToPage();
    }
}
