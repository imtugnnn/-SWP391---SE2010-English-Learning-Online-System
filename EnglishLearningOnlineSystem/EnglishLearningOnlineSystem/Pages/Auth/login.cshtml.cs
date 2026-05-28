using EnglishLearningOnlineSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnPost()
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == Email);
            if (user != null)
            {
                bool isValid =
                    BCrypt.Net.BCrypt.Verify(Password, user.Password);

                if (isValid)
                {
                    return RedirectToPage("/Homepage");
                }
            }
            return Page();
        }
    }
}