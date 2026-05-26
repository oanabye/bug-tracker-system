using BugTracker.Server.DTOs;

namespace BugTracker.Server.Services;

public interface IShareRequestService
{
    Task<bool> SendShareRequestAsync(CreateShareRequestDto dto, int fromDeveloperId);
    Task<List<ShareRequestResponseDto>> GetPendingRequestsAsync(int developerId);
    Task<bool> RespondToRequestAsync(int requestId, bool accept, int developerId);
}