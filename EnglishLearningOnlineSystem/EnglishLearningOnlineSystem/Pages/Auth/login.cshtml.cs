using EnglishLearningOnlineSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly WebDbContext _context;

        public LoginModel(WebDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; } = string.Empty;

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Email = Email.Trim();

            var user = _context.Users
                .FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                ModelState.AddModelError(nameof(Email), "Email is not registered.");
                return Page();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact support.");
                return Page();
            }

            bool isValid =
                BCrypt.Net.BCrypt.Verify(Password, user.Password);

            if (!isValid)
            {
                ModelState.AddModelError(nameof(Password), "Password is incorrect.");
                return Page();
            }

            return RedirectToPage("/Homepage");
        }
    }
}
