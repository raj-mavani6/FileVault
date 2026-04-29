using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Helpers;
using FileVault.Web.Models.ViewModels;
using FileVault.Web.Services;

namespace FileVault.Web.Controllers;

[Authorize]
public class FilesController : Controller
{
    private readonly IFileService _fileService;
    private readonly IFolderService _folderService;

    public FilesController(IFileService fileService, IFolderService folderService)
    {
        _fileService = fileService;
        _folderService = folderService;
    }

    public async Task<IActionResult> Index(string? folderId, int page = 1, string? sortBy = null,
        bool sortDesc = true, string? search = null, string? ext = null, string? tag = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var pageSize = 50;

        var (files, total) = await _fileService.GetFilesAsync(userId, folderId,
            page, pageSize, sortBy, sortDesc, search, ext, tag);
        var folders = await _folderService.GetChildrenAsync(userId, folderId);

        var breadcrumbs = new List<BreadcrumbItem>();
        if (folderId != null)
        {
            var ancestors = await _folderService.GetBreadcrumbsAsync(folderId, userId);
            breadcrumbs = ancestors.Select(a => new BreadcrumbItem { Id = a.Id, Name = a.Name }).ToList();
        }

        var model = new FileListViewModel
        {
            Files = files.Select(f => new FileItemViewModel
            {
                Id = f.Id,
                FileName = f.FileName,
                Extension = f.Extension,
                ContentType = f.ContentType,
                SizeBytes = f.SizeBytes,
                SizeFormatted = FileHelpers.FormatFileSize(f.SizeBytes),
                FolderId = f.FolderId,
                Tags = f.Tags,
                Description = f.Description,
                CreatedAt = f.CreatedAt,
                FileIcon = FileHelpers.GetFileIcon(f.Extension),
                IsPreviewable = FileHelpers.IsPreviewable(f.Extension),
                VersionNumber = f.VersionNumber
            }).ToList(),
            Folders = folders.Select(f => new FolderViewModel
            {
                Id = f.Id,
                Name = f.Name,
                ParentFolderId = f.ParentFolderId,
                CreatedAt = f.CreatedAt
            }).ToList(),
            CurrentFolderId = folderId,
            Breadcrumbs = breadcrumbs,
            Page = page,
            PageSize = pageSize,
            TotalFiles = total,
            SortBy = sortBy,
            SortDesc = sortDesc,
            SearchTerm = search,
            FilterExtension = ext,
            FilterTag = tag
        };

        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var file = await _fileService.GetByIdAsync(id, userId);
        if (file == null) return NotFound();

        var model = new FileItemViewModel
        {
            Id = file.Id,
            FileName = file.FileName,
            Extension = file.Extension,
            ContentType = file.ContentType,
            SizeBytes = file.SizeBytes,
            SizeFormatted = FileHelpers.FormatFileSize(file.SizeBytes),
            FolderId = file.FolderId,
            Tags = file.Tags,
            Description = file.Description,
            CreatedAt = file.CreatedAt,
            FileIcon = FileHelpers.GetFileIcon(file.Extension),
            IsPreviewable = FileHelpers.IsPreviewable(file.Extension),
            VersionNumber = file.VersionNumber
        };

        return View(model);
    }

    public IActionResult Upload(string? folderId)
    {
        ViewBag.FolderId = folderId;
        return View();
    }

    public async Task<IActionResult> Trash(int page = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (files, total) = await _fileService.GetTrashAsync(userId, page, 50);
        var folders = await _folderService.GetDeletedAsync(userId);

        var model = new FileListViewModel
        {
            Files = files.Select(f => new FileItemViewModel
            {
                Id = f.Id,
                FileName = f.FileName,
                Extension = f.Extension,
                SizeBytes = f.SizeBytes,
                SizeFormatted = FileHelpers.FormatFileSize(f.SizeBytes),
                IsDeleted = true,
                DeletedAt = f.DeletedAt,
                CreatedAt = f.CreatedAt,
                FileIcon = FileHelpers.GetFileIcon(f.Extension)
            }).ToList(),
            Folders = folders.Select(f => new FolderViewModel
            {
                Id = f.Id,
                Name = f.Name,
                ParentFolderId = f.ParentFolderId,
                CreatedAt = f.CreatedAt
            }).ToList(),
            Page = page,
            TotalFiles = total
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Download(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var (stream, contentType, fileName, size) = await _fileService.GetFileStreamAsync(id, userId);
            var safeFileName = FileHelpers.SanitizeFileName(fileName);
            return File(stream, contentType, safeFileName, enableRangeProcessing: true);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Preview(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var (stream, contentType, fileName, size) = await _fileService.GetFileStreamAsync(id, userId);
            var safeFileName = FileHelpers.SanitizeFileName(fileName);
            
            // Fix for non-ASCII filenames (e.g. Gujarati/Arabic) in headers
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = safeFileName,
                Inline = true
            };
            Response.Headers["Content-Disposition"] = contentDisposition.ToString();
            
            return File(stream, contentType, enableRangeProcessing: true);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetArchiveContents(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var (stream, contentType, fileName, size) = await _fileService.GetFileStreamAsync(id, userId);
            
            var files = new List<string>();
            try
            {
                using var reader = SharpCompress.Readers.ReaderFactory.Open(stream);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        files.Add($"{reader.Entry.Key} ({(reader.Entry.Size / 1024.0):0.##} KB)");
                    }
                    if (files.Count >= 50) break;
                }
            }
            catch { /* Not a valid archive */ }
            
            return Json(files);
        }
        catch { return NotFound(); }
    }
}
