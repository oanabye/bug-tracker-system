using BugTracker.Server.Models;

namespace BugTracker.Server.Repositories;

public interface IShareRequestRepository
{
    Task AddAsync(BugShareRequest request);
    Task<BugShareRequest?> GetByIdAsync(int id);
    Task<List<BugShareRequest>> GetPendingForDeveloperAsync(int developerId);
    Task SaveChangesAsync();
}