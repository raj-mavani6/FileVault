using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Services;

namespace FileVault.Web.Controllers.Api;

[ApiController]
[Route("api/folders")]
[Authorize]
public class FoldersApiController : ControllerBase
{
    private readonly IFolderService _folderService;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public FoldersApiController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFolderRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { error = "Folder name is required." });

        var folder = await _folderService.CreateAsync(UserId, req.ParentFolderId, req.Name.Trim());
        return Ok(new { folder.Id, folder.Name, folder.ParentFolderId, folder.CreatedAt });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Rename(string id, [FromBody] RenameFolderRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { error = "Folder name is required." });

        await _folderService.RenameAsync(id, UserId, req.Name.Trim());
        return Ok(new { message = "Folder renamed." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] bool permanent = false)
    {
        if (permanent)
            await _folderService.PermanentDeleteAsync(id, UserId);
        else
            await _folderService.SoftDeleteAsync(id, UserId);

        return Ok(new { message = "Folder deleted." });
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        await _folderService.RestoreAsync(id, UserId);
        return Ok(new { message = "Folder restored." });
    }

    [HttpPost("{id}/move")]
    public async Task<IActionResult> Move(string id, [FromBody] MoveFolderRequest req)
    {
        await _folderService.MoveAsync(id, UserId, req.NewParentFolderId);
        return Ok(new { message = "Folder moved." });
    }
}

public class CreateFolderRequest { public string Name { get; set; } = null!; public string? ParentFolderId { get; set; } }
public class RenameFolderRequest { public string Name { get; set; } = null!; }
public class MoveFolderRequest { public string? NewParentFolderId { get; set; } }
