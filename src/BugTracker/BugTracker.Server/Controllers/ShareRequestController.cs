using System.Security.Claims;
using BugTracker.Server.DTOs;
using BugTracker.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Developer")]
public class ShareRequestController : ControllerBase
{
    private readonly IShareRequestService _shareService;
    public ShareRequestController(IShareRequestService shareService)
        => _shareService = shareService;

    [HttpPost]
    public async Task<IActionResult> Send(CreateShareRequestDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _shareService.SendShareRequestAsync(dto, userId);
        return success ? Ok("Request sent.") : BadRequest("Failed to send request.");
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var requests = await _shareService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpPut("{id}/respond")]
    public async Task<IActionResult> Respond(int id, [FromBody] bool accept)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _shareService.RespondToRequestAsync(id, accept, userId);
        return success ? NoContent() : BadRequest("Failed to respond.");
    }
}