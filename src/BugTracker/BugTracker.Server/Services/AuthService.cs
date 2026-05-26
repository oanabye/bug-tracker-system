using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BugTracker.Server.DTOs;
using BugTracker.Server.Models;
using BugTracker.Server.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace BugTracker.Server.Services;

public class AuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByUsernameAsync(request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateToken(user);
        return new LoginResponse(token, user.Role.ToString(), user.Id);
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepo.GetByUsernameAsync(request.Username);
        if (existing != null) return false; // username already taken

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Developer // new registrations are always developers
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();
        return true;
    }
    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}