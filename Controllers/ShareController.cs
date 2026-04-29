using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Models.ViewModels;
using FileVault.Web.Services;
using FileVault.Web.Data.Repositories;
using MongoDB.Bson;
using FileVault.Web.Data.GridFs;

namespace FileVault.Web.Controllers;

public class ShareController : Controller
{
    private readonly IShareService _shareService;
    private readonly IFileRepository _fileRepo;
    private readonly IGridFsService _gridFs;

    public ShareController(IShareService shareService, IFileRepository fileRepo, IGridFsService gridFs)
    {
        _shareService = shareService;
        _fileRepo = fileRepo;
        _gridFs = gridFs;
    }

    [HttpGet("s/{token}")]
    public async Task<IActionResult> PublicView(string token)
    {
        var link = await _shareService.GetByTokenAsync(token);
        if (link == null)
            return View("PublicView", new ShareAccessViewModel { Token = token, ErrorMessage = "Share link not found." });

        var file = await _fileRepo.GetByIdAsync(link.FileId);

        var model = new ShareAccessViewModel
        {
            Token = token,
            RequiresPassword = link.PasswordHash != null,
            FileName = file?.FileName,
            FileSize = file != null ? Helpers.FileHelpers.FormatFileSize(file.SizeBytes) : null,
            AllowDownload = link.AllowDownload,
            IsExpired = link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow,
            IsRevoked = link.IsRevoked
        };

        if (model.IsExpired)
            model.ErrorMessage = "This share link has expired.";
        if (model.IsRevoked)
            model.ErrorMessage = "This share link has been revoked.";

        await _shareService.RecordAccessAsync(token);
        return View("PublicView", model);
    }

    [HttpPost("s/{token}")]
    public async Task<IActionResult> PublicViewAuth(string token, string? password)
    {
        var (valid, message) = await _shareService.ValidateShareAccessAsync(token, password);
        if (!valid)
        {
            var link = await _shareService.GetByTokenAsync(token);
            var file = link != null ? await _fileRepo.GetByIdAsync(link.FileId) : null;

            return View("PublicView", new ShareAccessViewModel
            {
                Token = token,
                RequiresPassword = true,
                FileName = file?.FileName,
                ErrorMessage = message
            });
        }

        return RedirectToAction(nameof(PublicDownload), new { token });
    }

    [HttpGet("s/{token}/download")]
    public async Task<IActionResult> PublicDownload(string token)
    {
        var link = await _shareService.GetByTokenAsync(token);
        if (link == null || !link.AllowDownload || link.IsRevoked)
            return NotFound();

        if (link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow)
            return NotFound();

        // If password protected, must have come through PublicViewAuth
        var file = await _fileRepo.GetByIdAsync(link.FileId);
        if (file == null) return NotFound();

        var gridFsId = ObjectId.Parse(file.GridFsFileId);
        var stream = await _gridFs.OpenDownloadStreamAsync(gridFsId);
        var safeFileName = file.FileName != null ? Helpers.FileHelpers.SanitizeFileName(file.FileName) : "downloaded_file";
        return File(stream, file.ContentType, safeFileName, enableRangeProcessing: true);
    }
}
