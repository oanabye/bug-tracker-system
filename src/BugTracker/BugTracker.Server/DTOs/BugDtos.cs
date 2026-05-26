using BugTracker.Server.Models;

namespace BugTracker.Server.DTOs;

public class CreateBugDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public string StepsToReproduce { get; set; } = string.Empty;
    public IFormFile? Photo { get; set; }
}

public class UpdateBugStatusDto
{
    public BugStatus Status { get; set; }
}

public class BugResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StepsToReproduce { get; set; } = string.Empty;
    public bool HasPhoto { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
}
public class AssignBugDto
{
    public int DeveloperId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
}
public class EditBugDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public string StepsToReproduce { get; set; } = string.Empty;
}

public class UpdatePasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class CreateShareRequestDto
{
    public int BugId { get; set; }
    public int ToDeveloperId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ShareRequestResponseDto
{
    public int Id { get; set; }
    public int BugId { get; set; }
    public string BugTitle { get; set; } = string.Empty;
    public string FromDeveloper { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}