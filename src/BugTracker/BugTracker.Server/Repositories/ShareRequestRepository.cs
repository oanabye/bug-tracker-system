using BugTracker.Server.Data;
using BugTracker.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Server.Repositories;

public class ShareRequestRepository : IShareRequestRepository
{
    private readonly AppDbContext _ctx;
    public ShareRequestRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(BugShareRequest request) =>
        await _ctx.BugShareRequests.AddAsync(request);

    public Task<BugShareRequest?> GetByIdAsync(int id) =>
        _ctx.BugShareRequests
            .Include(r => r.Bug)
            .Include(r => r.FromDeveloper)
            .Include(r => r.ToDeveloper)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<BugShareRequest>> GetPendingForDeveloperAsync(int developerId) =>
        _ctx.BugShareRequests
            .Include(r => r.Bug)
            .Include(r => r.FromDeveloper)
            .Where(r => r.ToDeveloperId == developerId &&
                        r.Status == ShareRequestStatus.Pending)
            .ToListAsync();

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}