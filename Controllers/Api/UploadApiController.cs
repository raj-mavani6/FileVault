using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Services;

namespace FileVault.Web.Controllers.Api;

[ApiController]
[Route("api/uploads")]
[Authorize]
[DisableRequestSizeLimit]
[RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
public class UploadApiController : ControllerBase
{
    private readonly IUploadService _uploadService;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public UploadApiController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateUploadRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FileName))
            return BadRequest(new { error = "File name is required." });

        try
        {
            var session = await _uploadService.InitiateAsync(
                UserId, req.FileName, req.ContentType ?? "application/octet-stream",
                req.TotalSize, req.ChunkSize, req.FolderId);

            return Ok(new
            {
                sessionId = session.Id,
                totalChunks = session.TotalChunks,
                chunkSize = session.ChunkSize,
                expiresAt = session.ExpiresAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{sessionId}/chunk/{chunkIndex}")]
    public async Task<IActionResult> UploadChunk(string sessionId, int chunkIndex)
    {
        var allDone = await _uploadService.UploadChunkAsync(sessionId, chunkIndex, Request.Body, UserId);
        return Ok(new { chunkIndex, allChunksUploaded = allDone });
    }

    [HttpPost("{sessionId}/complete")]
    public async Task<IActionResult> Complete(string sessionId)
    {
        var file = await _uploadService.CompleteAsync(sessionId, UserId);
        if (file == null)
            return BadRequest(new { error = "Failed to complete upload." });

        return Ok(new
        {
            fileId = file.Id,
            fileName = file.FileName,
            sizeBytes = file.SizeBytes,
            hash = file.HashSha256
        });
    }

    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> Abort(string sessionId)
    {
        await _uploadService.AbortAsync(sessionId, UserId);
        return Ok(new { message = "Upload aborted." });
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var sessions = await _uploadService.GetActiveSessionsAsync(UserId);
        return Ok(sessions.Select(s => new
        {
            s.Id, s.FileName, s.TotalSize, s.ChunkSize, s.TotalChunks,
            uploadedChunks = s.UploadedChunks.Count,
            s.Status, s.StartedAt, s.ExpiresAt
        }));
    }

    // Simple single-file upload for small files
    [HttpPost("simple")]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> SimpleUpload(IFormFile file, [FromForm] string? folderId)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        // For small files, use direct upload without chunking
        var session = await _uploadService.InitiateAsync(
            UserId, file.FileName, file.ContentType, file.Length,
            (int)Math.Min(file.Length, 5 * 1024 * 1024), folderId);

        await using var stream = file.OpenReadStream();
        await _uploadService.UploadChunkAsync(session.Id, 0, stream, UserId);

        var result = await _uploadService.CompleteAsync(session.Id, UserId);
        return Ok(new { fileId = result?.Id, fileName = result?.FileName, sizeBytes = result?.SizeBytes });
    }
}

public class InitiateUploadRequest
{
    public string FileName { get; set; } = null!;
    public string? ContentType { get; set; }
    public long TotalSize { get; set; }
    public int ChunkSize { get; set; }
    public string? FolderId { get; set; }
}
