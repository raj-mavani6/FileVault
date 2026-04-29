using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Models.ViewModels;
using FileVault.Web.Services;

namespace FileVault.Web.Controllers.Api;

[ApiController]
[Route("api/shares")]
[Authorize]
public class ShareApiController : ControllerBase
{
    private readonly IShareService _shareService;
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public ShareApiController(IShareService shareService)
    {
        _shareService = shareService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShareCreateViewModel model)
    {
        DateTime? expiry = model.ExpiryDays > 0
            ? DateTime.UtcNow.AddDays(model.ExpiryDays) : null;

        var link = await _shareService.CreateShareLinkAsync(
            model.FileId, UserId, model.AllowDownload, expiry, model.Password);

        return Ok(new
        {
            id = link.Id,
            token = link.Token,
            url = $"{Request.Scheme}://{Request.Host}/s/{link.Token}",
            expiresAt = link.ExpiresAt,
            hasPassword = link.PasswordHash != null
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyShares([FromQuery] int page = 1)
    {
        var shares = await _shareService.GetUserSharesAsync(UserId, page);
        var total = await _shareService.CountUserSharesAsync(UserId);

        return Ok(new
        {
            shares = shares.Select(s => new
            {
                s.Id, s.FileId, s.Token, s.ExpiresAt,
                s.AllowDownload, s.AccessCount, s.IsRevoked, s.CreatedAt,
                url = $"{Request.Scheme}://{Request.Host}/s/{s.Token}"
            }),
            total,
            page
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Revoke(string id)
    {
        await _shareService.RevokeAsync(id, UserId);
        return Ok(new { message = "Share link revoked." });
    }
}
