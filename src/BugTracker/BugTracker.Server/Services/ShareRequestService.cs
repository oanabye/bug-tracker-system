using BugTracker.Server.DTOs;
using BugTracker.Server.Models;
using BugTracker.Server.Repositories;

namespace BugTracker.Server.Services;

public class ShareRequestService : IShareRequestService
{
    private readonly IShareRequestRepository _shareRepo;
    private readonly IBugRepository _bugRepo;

    public ShareRequestService(IShareRequestRepository shareRepo, IBugRepository bugRepo)
    {
        _shareRepo = shareRepo;
        _bugRepo = bugRepo;
    }

    public async Task<bool> SendShareRequestAsync(CreateShareRequestDto dto, int fromDeveloperId)
    {
        var bug = await _bugRepo.GetByIdAsync(dto.BugId);
        if (bug == null) return false;

        await _shareRepo.AddAsync(new BugShareRequest
        {
            BugId = dto.BugId,
            FromDeveloperId = fromDeveloperId,
            ToDeveloperId = dto.ToDeveloperId,
            Message = dto.Message,
            Status = ShareRequestStatus.Pending
        });

        await _shareRepo.SaveChangesAsync();
        return true;
    }

    public async Task<List<ShareRequestResponseDto>> GetPendingRequestsAsync(int developerId)
    {
        var requests = await _shareRepo.GetPendingForDeveloperAsync(developerId);
        return requests.Select(r => new ShareRequestResponseDto
        {
            Id = r.Id,
            BugId = r.BugId,
            BugTitle = r.Bug.Title,
            FromDeveloper = r.FromDeveloper.Username,
            Message = r.Message,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<bool> RespondToRequestAsync(int requestId, bool accept, int developerId)
    {
        var request = await _shareRepo.GetByIdAsync(requestId);
        if (request == null || request.ToDeveloperId != developerId) return false;

        request.Status = accept ? ShareRequestStatus.Accepted : ShareRequestStatus.Declined;

        if (accept)
        {
            var bug = await _bugRepo.GetByIdAsync(request.BugId);
            if (bug != null)
            {
                bug.AssignedToId = developerId;
                await _bugRepo.SaveChangesAsync();
            }
        }

        await _shareRepo.SaveChangesAsync();
        return true;
    }
}