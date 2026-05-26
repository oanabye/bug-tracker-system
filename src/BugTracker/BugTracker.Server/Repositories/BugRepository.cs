using BugTracker.Server.Data;
using BugTracker.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Server.Repositories;

public class BugRepository : IBugRepository
{
    private readonly AppDbContext _ctx;
    public BugRepository(AppDbContext ctx) => _ctx = ctx;

    public Task<List<Bug>> GetAllAsync() =>
        _ctx.Bugs.Include(b => b.ReportedBy).Include(b => b.AssignedTo).ToListAsync();

    public Task<Bug?> GetByIdAsync(int id) =>
        _ctx.Bugs.Include(b => b.ReportedBy).Include(b => b.AssignedTo)
                 .FirstOrDefaultAsync(b => b.Id == id);

    public async Task AddAsync(Bug bug) => await _ctx.Bugs.AddAsync(bug);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}