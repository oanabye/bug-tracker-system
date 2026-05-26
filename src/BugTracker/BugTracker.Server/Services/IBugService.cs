using BugTracker.Server.DTOs;
using BugTracker.Server.Models;

namespace BugTracker.Server.Services;

public interface IBugService
{
    Task<List<BugResponseDto>> GetAllBugsAsync();
    Task<BugResponseDto> CreateBugAsync(CreateBugDto dto, int reportedById);
    Task<bool> UpdateStatusAsync(int bugId, BugStatus newStatus, int requestingUserId, string role);
    Task<byte[]?> GetPhotoAsync(int bugId);
    Task ImportFromXmlAsync(Stream xmlStream);
    Task<bool> AssignBugAsync(int bugId, int developerId);
    Task<List<UserDto>> GetDevelopersAsync();
    Task<List<BugResponseDto>> GetBugsByDeveloperAsync(int developerId);
    Task<bool> EditBugAsync(int bugId, EditBugDto dto);
}