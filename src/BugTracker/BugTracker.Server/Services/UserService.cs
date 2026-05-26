using BugTracker.Server.DTOs;
using BugTracker.Server.Models;
using BugTracker.Server.Repositories;

namespace BugTracker.Server.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    public UserService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<List<UserDto>> GetAllDevelopersAsync()
    {
        var devs = await _userRepo.GetAllDevelopersAsync();
        return devs.Select(d => new UserDto { Id = d.Id, Username = d.Username }).ToList();
    }

    public async Task<bool> AddDeveloperAsync(string username, string password)
    {
        var existing = await _userRepo.GetByUsernameAsync(username);
        if (existing != null) return false;

        await _userRepo.AddAsync(new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Developer
        });
        await _userRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDeveloperAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Developer) return false;
        await _userRepo.DeleteAsync(id);
        await _userRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePasswordAsync(int id, string newPassword)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepo.SaveChangesAsync();
        return true;
    }
}