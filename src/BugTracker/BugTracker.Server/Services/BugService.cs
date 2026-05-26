using BugTracker.Server.DTOs;
using BugTracker.Server.Models;
using BugTracker.Server.Repositories;
using System.Xml.Linq;

namespace BugTracker.Server.Services;

public class BugService : IBugService
{
    private readonly IBugRepository _bugRepo;
    private readonly IUserRepository _userRepo;

    public BugService(IBugRepository bugRepo, IUserRepository userRepo)
    {
        _bugRepo = bugRepo;
        _userRepo = userRepo;
    }

    public async Task<List<BugResponseDto>> GetAllBugsAsync()
    {
        var bugs = await _bugRepo.GetAllAsync();
        return bugs.Select(ToDto).ToList();
    }

    public async Task<BugResponseDto> CreateBugAsync(CreateBugDto dto, int reportedById)
    {
        byte[]? photoBytes = null;
        if (dto.Photo != null)
        {
            using var ms = new MemoryStream();
            await dto.Photo.CopyToAsync(ms);
            photoBytes = ms.ToArray();
        }

        var bug = new Bug
        {
            Title = dto.Title,
            Description = dto.Description,
            Severity = dto.Severity,
            StepsToReproduce = dto.StepsToReproduce,
            Photo = photoBytes,
            ReportedById = reportedById,
            CreatedAt = DateTime.UtcNow
        };

        await _bugRepo.AddAsync(bug);
        await _bugRepo.SaveChangesAsync();
        return ToDto(bug);
    }

    public async Task<bool> UpdateStatusAsync(int bugId, BugStatus newStatus, int requestingUserId, string role)
    {
        var bug = await _bugRepo.GetByIdAsync(bugId);
        if (bug == null) return false;

        // Admin poate modifica orice bug
        if (role == "Administrator")
        {
            bug.Status = newStatus;
            await _bugRepo.SaveChangesAsync();
            return true;
        }

        // Developer poate modifica doar bug-urile asignate lui
        if (bug.AssignedToId != requestingUserId)
            return false;

        bug.Status = newStatus;
        await _bugRepo.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]?> GetPhotoAsync(int bugId)
    {
        var bug = await _bugRepo.GetByIdAsync(bugId);
        return bug?.Photo;
    }

    public async Task ImportFromXmlAsync(Stream xmlStream)
    {
        var doc = XDocument.Load(xmlStream);
        var bugs = doc.Descendants("Bug").Select(x => new Bug
        {
            Title = x.Element("Title")?.Value ?? "Imported Bug",
            Description = x.Element("Description")?.Value ?? "",
            StepsToReproduce = x.Element("StepsToReproduce")?.Value ?? "",
            Severity = Enum.TryParse<Severity>(x.Element("Severity")?.Value, out var sev) ? sev : Severity.Low,
            Status = BugStatus.New,
            ReportedById = 1, // admin
            CreatedAt = DateTime.UtcNow
        });

        foreach (var bug in bugs)
            await _bugRepo.AddAsync(bug);

        await _bugRepo.SaveChangesAsync();
    }

    private static BugResponseDto ToDto(Bug b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Description = b.Description,
        Severity = b.Severity.ToString(),
        Status = b.Status.ToString(),
        StepsToReproduce = b.StepsToReproduce,
        HasPhoto = b.Photo != null,
        CreatedAt = b.CreatedAt,
        ReportedBy = b.ReportedBy?.Username ?? "",
        AssignedTo = b.AssignedTo?.Username
    };

    public async Task<bool> AssignBugAsync(int bugId, int developerId)
    {
        var bug = await _bugRepo.GetByIdAsync(bugId);
        if (bug == null) return false;
        bug.AssignedToId = developerId;
        await _bugRepo.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDto>> GetDevelopersAsync()
    {
        var devs = await _userRepo.GetAllDevelopersAsync();
        return devs.Select(d => new UserDto { Id = d.Id, Username = d.Username }).ToList();
    }
    public async Task<List<BugResponseDto>> GetBugsByDeveloperAsync(int developerId)
    {
        var bugs = await _bugRepo.GetAllAsync();
        return bugs.Where(b => b.AssignedToId == developerId).Select(ToDto).ToList();
    }
    public async Task<bool> EditBugAsync(int bugId, EditBugDto dto)
    {
        var bug = await _bugRepo.GetByIdAsync(bugId);
        if (bug == null) return false;
        bug.Title = dto.Title;
        bug.Description = dto.Description;
        bug.Severity = dto.Severity;
        bug.StepsToReproduce = dto.StepsToReproduce;
        await _bugRepo.SaveChangesAsync();
        return true;
    }
}