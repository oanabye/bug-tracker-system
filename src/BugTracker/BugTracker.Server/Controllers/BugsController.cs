using System.Security.Claims;
using BugTracker.Server.DTOs;
using BugTracker.Server.Models;
using BugTracker.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BugsController : ControllerBase
{
    private readonly IBugService _bugService;
    public BugsController(IBugService bugService) => _bugService = bugService;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _bugService.GetAllBugsAsync());

    [HttpPost]
    [Authorize(Roles = "Developer,Administrator")]
    public async Task<IActionResult> Create([FromForm] CreateBugDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bugService.CreateBugAsync(dto, userId);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Developer,Administrator")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateBugStatusDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var updated = await _bugService.UpdateStatusAsync(id, dto.Status, userId, role);

        if (!updated)
            return BadRequest("You can only update status for bugs assigned to you.");

        return NoContent();
    }

    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var photo = await _bugService.GetPhotoAsync(id);
        if (photo == null) return NotFound();
        return File(photo, "image/jpeg");
    }

    [HttpPost("import-xml")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ImportXml(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file provided");
        using var stream = file.OpenReadStream();
        await _bugService.ImportFromXmlAsync(stream);
        return Ok("Import successful");
    }

    [HttpPut("{id}/assign")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AssignBug(int id, AssignBugDto dto)
    {
        var updated = await _bugService.AssignBugAsync(id, dto.DeveloperId);
        return updated ? NoContent() : NotFound();
    }

    [HttpGet("developers")]
    [Authorize(Roles = "Developer, Administrator")]
    public async Task<IActionResult> GetDevelopers()
    {
        var devs = await _bugService.GetDevelopersAsync();
        return Ok(devs);
    }

    [HttpGet("my-bugs")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> GetMyBugs()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bugService.GetBugsByDeveloperAsync(userId);
        return Ok(result);
    }

}