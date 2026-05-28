using System.ComponentModel.DataAnnotations;
using EnglishLearningOnlineSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly WebDbContext _context;

        public RegisterModel(WebDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RegisterInput Input { get; set; } = new();

        public List<SelectListItem> RoleOptions { get; private set; } = new();

        public async Task OnGetAsync()
        {
            await LoadRoleOptionsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadRoleOptionsAsync();

            var role = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.Id == Input.RoleId &&
                    (r.Name == "Student" || r.Name == "Parent"));

            if (role == null)
            {
                ModelState.AddModelError("Input.RoleId", "Please choose Student or Parent.");
            }

            if (await _context.Users.AnyAsync(u => u.Username == Input.Username))
            {
                ModelState.AddModelError("Input.Username", "Username is already taken.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Email is already registered.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new User
            {
                Username = Input.Username.Trim(),
                Email = Input.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(Input.Password),
                BirthDate = Input.BirthDate,
                IsActive = true,
                RoleId = Input.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Auth/Login");
        }

        private async Task LoadRoleOptionsAsync()
        {
            RoleOptions = await _context.Roles
                .Where(r => r.Name == "Student" || r.Name == "Parent")
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name
                })
                .ToListAsync();
        }

        public class RegisterInput
        {
            [Required]
            [StringLength(50)]
            public string Username { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [StringLength(100)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Date)]
            public DateTime? BirthDate { get; set; }

            [Required]
            public int RoleId { get; set; }
        }
    }
}
