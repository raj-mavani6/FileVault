using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Helpers;
using FileVault.Web.Models.ViewModels;
using FileVault.Web.Services;

namespace FileVault.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IFileService _fileService;
    private readonly IFolderService _folderService;
    private readonly IShareService _shareService;

    public DashboardController(IFileService fileService, IFolderService folderService, IShareService shareService)
    {
        _fileService = fileService;
        _folderService = folderService;
        _shareService = shareService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var totalFiles = await _fileService.GetUserFileCountAsync(userId);
        var totalFolders = await _folderService.CountAsync(userId);
        var storageUsed = await _fileService.GetUserStorageAsync(userId);
        var sharedCount = await _shareService.CountUserSharesAsync(userId);
        var (trashFiles, trashCount) = await _fileService.GetTrashAsync(userId, 1, 1);
        var recentFiles = await _fileService.GetRecentAsync(userId, 8);
        var extensionStats = await _fileService.GetExtensionStatsAsync(userId);

        var model = new DashboardViewModel
        {
            TotalFiles = totalFiles,
            TotalFolders = totalFolders,
            StorageUsedBytes = storageUsed,
            StorageUsedFormatted = FileHelpers.FormatFileSize(storageUsed),
            SharedCount = sharedCount,
            TrashCount = trashCount,
            RecentFiles = recentFiles.Select(f => new FileItemViewModel
            {
                Id = f.Id,
                FileName = f.FileName,
                Extension = f.Extension,
                SizeBytes = f.SizeBytes,
                SizeFormatted = FileHelpers.FormatFileSize(f.SizeBytes),
                CreatedAt = f.CreatedAt,
                FileIcon = FileHelpers.GetFileIcon(f.Extension),
                IsPreviewable = FileHelpers.IsPreviewable(f.Extension)
            }).ToList(),
            FileTypeStats = extensionStats
        };

        return View(model);
    }
}
