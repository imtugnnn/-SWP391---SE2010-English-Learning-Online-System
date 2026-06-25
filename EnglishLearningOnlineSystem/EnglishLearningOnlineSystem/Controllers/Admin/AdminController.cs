using Microsoft.AspNetCore.Mvc;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly AppDbContext _context;

    public AdminController(IUserService userService, IRoleService roleService, AppDbContext context)
    {
        _userService = userService;
        _roleService = roleService;
        _context = context;
    }

    public IActionResult Dashboard()
    {
        return View("AdminDashboard");
    }
    
    public async Task<IActionResult> UserManagement()
    {
        var usersResult = await _userService.GetAllAsync();
        var roles = await _roleService.GetAllAsync();

        ViewBag.Roles = roles;
        var users = usersResult.Succeeded ? usersResult.Data : new List<User>();
        return View("~/Views/Admin/UserManagement/Index.cshtml", users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _userService.CreateAsync(vm);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi tạo người dùng." });
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> EditUser([FromBody] UserEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _userService.UpdateAsync(vm);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi cập nhật người dùng." });
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var userResult = await _userService.GetByIdAsync(id);
        if (!userResult.Succeeded || userResult.Data == null)
        {
            return Json(new { success = false, message = "Không tìm thấy người dùng." });
        }

        var user = userResult.Data;
        var vm = new UserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            BirthDate = user.BirthDate,
            IsActive = !user.IsActive, // Toggle
            RoleId = user.RoleId,
            Password = null
        };

        var updateResult = await _userService.UpdateAsync(vm);
        if (!updateResult.Succeeded)
        {
            return Json(new { success = false, message = updateResult.ErrorMessage ?? "Lỗi khi cập nhật trạng thái." });
        }

        return Json(new { success = true, isActive = vm.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi xóa người dùng." });
        }

        return Json(new { success = true });
    }
}   
