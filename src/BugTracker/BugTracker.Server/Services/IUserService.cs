using BugTracker.Server.DTOs;

namespace BugTracker.Server.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllDevelopersAsync();
    Task<bool> AddDeveloperAsync(string username, string password);
    Task<bool> DeleteDeveloperAsync(int id);
    Task<bool> UpdatePasswordAsync(int id, string newPassword);
}