using BugTracker.Server.Models;

namespace BugTracker.Server.Repositories;

public interface IBugRepository
{
    Task<List<Bug>> GetAllAsync();
    Task<Bug?> GetByIdAsync(int id);
    Task AddAsync(Bug bug);
    Task SaveChangesAsync();
}