using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Models.ViewModels;
using FileVault.Web.Services;
using FileVault.Web.Helpers;

namespace FileVault.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAuthService _authService;
    private readonly IFileService _fileService;

    public ProfileController(IAuthService authService, IFileService fileService)
    {
        _authService = authService;
        _fileService = fileService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();

        var storage = await _fileService.GetUserStorageAsync(userId);
        var fileCount = await _fileService.GetUserFileCountAsync(userId);

        var model = new ProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            StorageUsed = FileHelpers.FormatFileSize(storage),
            TotalFiles = fileCount
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _authService.UpdateProfileAsync(userId, model.FullName);
        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the errors.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (success, message) = await _authService.ChangePasswordAsync(
            userId, model.CurrentPassword, model.NewPassword);

        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
