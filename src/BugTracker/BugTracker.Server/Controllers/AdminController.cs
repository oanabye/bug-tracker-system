using BugTracker.Server.DTOs;
using BugTracker.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBugService _bugService;

    public AdminController(IUserService userService, IBugService bugService)
    {
        _userService = userService;
        _bugService = bugService;
    }

    [HttpGet("developers")]
    public async Task<IActionResult> GetDevelopers() =>
        Ok(await _userService.GetAllDevelopersAsync());

    [HttpPost("developers")]
    public async Task<IActionResult> AddDeveloper(RegisterRequest request)
    {
        var success = await _userService.AddDeveloperAsync(request.Username, request.Password);
        return success ? Ok("Developer added.") : Conflict("Username already exists.");
    }

    [HttpDelete("developers/{id}")]
    public async Task<IActionResult> DeleteDeveloper(int id)
    {
        var success = await _userService.DeleteDeveloperAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpPut("developers/{id}/password")]
    public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordDto dto)
    {
        var success = await _userService.UpdatePasswordAsync(id, dto.NewPassword);
        return success ? NoContent() : NotFound();
    }

    [HttpPut("bugs/{id}")]
    public async Task<IActionResult> EditBug(int id, EditBugDto dto)
    {
        var success = await _bugService.EditBugAsync(id, dto);
        return success ? NoContent() : NotFound();
    }
}