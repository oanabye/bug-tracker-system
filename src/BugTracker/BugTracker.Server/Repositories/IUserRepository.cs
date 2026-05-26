using BugTracker.Server.Models;

namespace BugTracker.Server.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task<List<User>> GetAllDevelopersAsync();
    Task<User?> GetByIdAsync(int id);
    Task DeleteAsync(int id);
}