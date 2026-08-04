using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UsersController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _userService.GetAllAsync();
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Cannot load users.");
                return View(new List<User>());
            }
            return View(result.Data!);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateRolesDropDownList();
            return View(new UserCreateViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateRolesDropDownList(vm.RoleId);
                return View(vm);
            }

            var result = await _userService.CreateAsync(vm);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Create user failed.");
                await PopulateRolesDropDownList(vm.RoleId);
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var result = await _userService.GetByIdAsync(id.Value);
            if (!result.Succeeded || result.Data == null) return NotFound();

            var u = result.Data;

            var vm = new UserEditViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.StudentProfile?.FullName,
                Email = u.Email,
                BirthDate = u.BirthDate,
                IsActive = u.IsActive,
                RoleId = u.RoleId,
                Password = null
            };

            await PopulateRolesDropDownList(vm.RoleId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateRolesDropDownList(vm.RoleId);
                return View(vm);
            }

            var result = await _userService.UpdateAsync(vm);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Update user failed.");
                await PopulateRolesDropDownList(vm.RoleId);
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateRolesDropDownList(object? selectedRole = null)
        {
            var roles = await _roleService.GetAllAsync();
            ViewBag.RoleId = new SelectList(roles, "Id", "Name", selectedRole);
        }
    }
}
