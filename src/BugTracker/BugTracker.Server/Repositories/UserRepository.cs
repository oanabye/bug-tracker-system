using BugTracker.Server.Data;
using BugTracker.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Server.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _ctx;
    public UserRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<User?> GetByUsernameAsync(string username) =>
        _ctx.Users.FirstOrDefaultAsync(u => u.Username == username);
    public Task<List<User>> GetAllDevelopersAsync() =>
    _ctx.Users.Where(u => u.Role == UserRole.Developer).ToListAsync();
    public async Task AddAsync(User user) => await _ctx.Users.AddAsync(user);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
    public Task<User?> GetByIdAsync(int id) =>
    _ctx.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task DeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user != null) _ctx.Users.Remove(user);
    }
}