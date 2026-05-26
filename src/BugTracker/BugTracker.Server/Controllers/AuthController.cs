using BugTracker.Server.DTOs;
using BugTracker.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null) return Unauthorized("Invalid credentials");
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        var success = await _authService.RegisterAsync(request);
        if (!success) return Conflict("Username already exists.");
        return Ok("Developer account created.");
    }
}