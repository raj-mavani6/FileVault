using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Services;
using FileVault.Web.Helpers;

namespace FileVault.Web.Controllers.Api;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesApiController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IFolderService _folderService;

    public FilesApiController(IFileService fileService, IFolderService folderService)
    {
        _fileService = fileService;
        _folderService = folderService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetFiles([FromQuery] string? folderId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50, [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true, [FromQuery] string? search = null,
        [FromQuery] string? ext = null, [FromQuery] string? tag = null)
    {
        var (files, total) = await _fileService.GetFilesAsync(UserId, folderId,
            page, pageSize, sortBy, sortDesc, search, ext, tag);
        var folders = await _folderService.GetChildrenAsync(UserId, folderId);

        return Ok(new
        {
            files = files.Select(f => new
            {
                f.Id, f.FileName, f.Extension, f.ContentType, f.SizeBytes,
                sizeFormatted = FileHelpers.FormatFileSize(f.SizeBytes),
                f.FolderId, f.Tags, f.Description, f.VersionNumber,
                f.CreatedAt, f.UpdatedAt,
                fileIcon = FileHelpers.GetFileIcon(f.Extension),
                isPreviewable = FileHelpers.IsPreviewable(f.Extension)
            }),
            folders = folders.Select(f => new { f.Id, f.Name, f.ParentFolderId, f.CreatedAt }),
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFile(string id)
    {
        var file = await _fileService.GetByIdAsync(id, UserId);
        if (file == null) return NotFound();

        return Ok(new
        {
            file.Id, file.FileName, file.Extension, file.ContentType, file.SizeBytes,
            sizeFormatted = FileHelpers.FormatFileSize(file.SizeBytes),
            file.FolderId, file.Tags, file.Description, file.HashSha256,
            file.VersionNumber, file.CreatedAt, file.UpdatedAt,
            fileIcon = FileHelpers.GetFileIcon(file.Extension),
            isPreviewable = FileHelpers.IsPreviewable(file.Extension)
        });
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(string id)
    {
        var (stream, contentType, fileName, size) = await _fileService.GetFileStreamAsync(id, UserId);
        var safeFileName = FileHelpers.SanitizeFileName(fileName);
        return File(stream, contentType, safeFileName, enableRangeProcessing: true);
    }

    [HttpGet("{id}/preview")]
    public async Task<IActionResult> Preview(string id)
    {
        var (stream, contentType, fileName, size) = await _fileService.GetFileStreamAsync(id, UserId);
        var safeFileName = FileHelpers.SanitizeFileName(fileName);
        
        var contentDisposition = new System.Net.Mime.ContentDisposition
        {
            FileName = safeFileName,
            Inline = true
        };
        Response.Headers["Content-Disposition"] = contentDisposition.ToString();
        
        return File(stream, contentType, enableRangeProcessing: true);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFile(string id, [FromBody] UpdateFileRequest req)
    {
        var file = await _fileService.GetByIdAsync(id, UserId);
        if (file == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.FileName))
            file.FileName = FileHelpers.SanitizeFileName(req.FileName);
        if (req.Tags != null)
            file.Tags = req.Tags;
        if (req.Description != null)
            file.Description = req.Description;
        if (req.FolderId != null)
            file.FolderId = req.FolderId == "" ? null : req.FolderId;

        await _fileService.UpdateAsync(file, UserId);
        return Ok(new { message = "File updated." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(string id, [FromQuery] bool permanent = false)
    {
        if (permanent)
            await _fileService.PermanentDeleteAsync(id, UserId);
        else
            await _fileService.SoftDeleteAsync(id, UserId);

        return Ok(new { message = permanent ? "File permanently deleted." : "File moved to trash." });
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreFile(string id)
    {
        await _fileService.RestoreAsync(id, UserId);
        return Ok(new { message = "File restored." });
    }

    [HttpPost("empty-trash")]
    public async Task<IActionResult> EmptyTrash()
    {
        await _fileService.EmptyTrashAsync(UserId);
        return Ok(new { message = "Trash emptied." });
    }
}

public class UpdateFileRequest
{
    public string? FileName { get; set; }
    public List<string>? Tags { get; set; }
    public string? Description { get; set; }
    public string? FolderId { get; set; }
}
